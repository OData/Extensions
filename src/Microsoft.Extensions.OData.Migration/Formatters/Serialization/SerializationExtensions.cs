// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal static class SerializationExtensions
    {
        /// <summary>
        /// Replace the inner HTTP response stream with substituteStream using reflection.
        /// 
        /// The stream needs to be substituted because the request body needs to be translated before passed on to the base deserialization classes
        /// to take advantage of OData V4 model validation.  Unfortunately, although it is guaranteed to exist, the stream is marked private
        /// in the ODataMessageReader, so reflection must be used to modify it.
        /// </summary>
        /// <param name="writer">ODataMessageWriter which has not written yet.</param>
        /// <param name="substituteStream">Replacement stream.</param>
        public static Stream SubstituteResponseStream(this ODataMessageWriter writer, Stream substituteStream)
        {
            FieldInfo messageField = writer.GetType().GetField("message", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            object message = messageField.GetValue(writer);
            FieldInfo requestMessageField = message.GetType().GetField("responseMessage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            object requestMessage = requestMessageField.GetValue(message);
            FieldInfo streamField = requestMessage.GetType().GetField("_stream", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            Stream originalStream = (Stream)streamField.GetValue(requestMessage);
            streamField.SetValue(requestMessage, substituteStream);
            return originalStream;
        }

        /// <summary>
        /// Substitute the stream from the messageWriter before writing, then read out the substituted stream
        /// and translate, then write to the original response stream.
        /// </summary>
        /// <param name="messageWriter">ODataMessageWriter</param>
        /// <param name="edmType">Type of object to be serialized</param>
        /// <param name="writeAction">Writer's action to write to stream</param>
        public static void PreemptivelyTranslateResponseStream(this ODataMessageWriter messageWriter, IEdmTypeReference edmType, Action<ODataMessageWriter> writeAction)
        {
            Stream substituteStream = new MemoryStream();
            Stream originalStream = messageWriter.SubstituteResponseStream(substituteStream);
            writeAction(messageWriter);

            // read fake stream, walk translate json object, add item
            JToken responsePayload;
            StreamReader reader = new StreamReader(substituteStream);

            substituteStream.Seek(0, SeekOrigin.Begin);
            responsePayload = JToken.Parse(reader.ReadToEnd());
            WalkTranslateResponse(responsePayload, edmType);

            // Write to actual stream
            // We cannot dispose of the stream, the outside methods will close it
            StreamWriter streamWriter = new StreamWriter(originalStream);
            JsonTextWriter writer = new JsonTextWriter(streamWriter);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, responsePayload);
            writer.Flush();

            messageWriter.SubstituteResponseStream(originalStream);
        }

        /// <summary>
        /// Determine and return the EdmType of given type given the SerializerContext.
        /// </summary>
        /// <param name="context">ODataSerializerContext.</param>
        /// <param name="instance">Value of instance.</param>
        /// <param name="type">Type to determine equivalent EdmType from.</param>
        /// <returns>Equivalent edm type to given type</returns>
        public static IEdmTypeReference GetEdmType(this ODataSerializerContext context, object instance, Type type)
        {
            IEdmTypeReference edmType;

            IEdmObject edmObject = instance as IEdmObject;
            if (edmObject != null)
            {
                edmType = edmObject.GetEdmType();
                if (edmType == null)
                {
                    throw new InvalidOperationException("Edm type cannot be null");
                }
            }
            else
            {
                if (context.Model == null)
                {
                    throw new InvalidOperationException("Edm model cannot be null");
                }

                ClrTypeCache typeMappingCache = context.Model.GetTypeMappingCache();
                edmType = typeMappingCache.GetEdmType(type, context.Model);

                if (edmType == null)
                {
                    if (instance != null)
                    {
                        edmType = typeMappingCache.GetEdmType(instance.GetType(), context.Model);
                    }

                    if (edmType == null)
                    {
                        throw new InvalidOperationException("Clr type " + type.FullName + " not in model");
                    }
                }
                else if (instance != null)
                {
                    IEdmTypeReference actualType = typeMappingCache.GetEdmType(instance.GetType(), context.Model);
                    if (actualType != null && actualType != edmType)
                    {
                        edmType = actualType;
                    }
                }
            }

            return edmType;
        }


        // Walk the JSON body and format instance annotations, and change incoming types based on expected types.
        private static void WalkTranslateResponse(this JToken node, IEdmTypeReference edmType)
        {
            if (node == null) return;

            if (edmType.IsCollection() && node.Type == JTokenType.Object && node["value"] != null)
            {
                // Special case where entity type is a collection
                WalkTranslateResponse(node["value"], edmType);
            }
            else
            {
                if (node.Type == JTokenType.Object)
                {
                    JObject obj = (JObject)node;
                    IEdmStructuredTypeReference structuredType = edmType.AsStructured();
                    foreach (JProperty child in node.Children<JProperty>().ToList())
                    {
                        IEdmProperty property = structuredType.FindProperty(child.Name);

                        if (property != null &&
                            property.Type.TypeKind() == EdmTypeKind.Primitive &&
                            ((IEdmPrimitiveType)property.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.Int64)
                        {
                            obj[child.Name] = obj[child.Name].ToString();
                        }
                        else if (property != null)
                        {
                            // If type is not IEdmStructuredTypeReference or IEdmCollectionTypeReference, then won't need to convert.
                            if (property.Type.TypeKind() == EdmTypeKind.Collection)
                            {
                                WalkTranslateResponse(child.Value, property.Type as IEdmCollectionTypeReference);
                            }
                            else if (property.Type.TypeKind() == EdmTypeKind.Entity ||
                                     property.Type.TypeKind() == EdmTypeKind.Complex)
                            {
                                WalkTranslateResponse(child.Value, property.Type as IEdmStructuredTypeReference);
                            }
                        }
                    }
                }
                else if (node.Type == JTokenType.Array)
                {
                    JArray items = (JArray)node;
                    IEdmCollectionTypeReference collectionType = (IEdmCollectionTypeReference)edmType;
                    IEdmTypeReference elementType = collectionType.Definition.AsElementType().ToEdmTypeReference();

                    if (elementType != null)
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (elementType.IsComplex() || elementType.IsEntity() || elementType.IsCollection())
                            {
                                items[i].WalkTranslateResponse(elementType);
                            }
                            else
                            {
                                // Do translation of V3 formatted types to V4 formatted types.
                                if (items[i].Type == JTokenType.String && elementType.IsInt64())
                                {
                                    items[i] = new JValue(items[i].ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
