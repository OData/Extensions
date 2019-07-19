﻿// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// This customized deserializer for resources will modify request bodies that represent OData entities
    /// to convert OData V3 request body conventions to V4 request body conventions (e.g. quoted longs to longs)
    /// </summary>
    public class ODataMigrationResourceDeserializer : ODataResourceDeserializer
    {
        public ODataMigrationResourceDeserializer(ODataDeserializerProvider provider)
            : base(provider)
        {
        }

        /// <summary>
        /// If the request body is V3, preempts the base deserializer by first modifying the JSON request body, then
        /// swapping out the HTTP request stream with the modified body that can be successfully read by the base deserializer.
        /// </summary>
        /// <param name="messageReader">An ODataMessageReader</param>
        /// <param name="type">Unused by this read method</param>
        /// <param name="readContext">Context to track state and settings of deserialization</param>
        /// <returns>Deserialized object from request body</returns>
        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (readContext.Request.Headers.ContainsKey("DataServiceVersion") || readContext.Request.Headers.ContainsKey("MaxDataServiceVersion"))
            {
                // Read the entire stream and convert to json
                JToken json;
                using (StreamReader reader = new StreamReader(readContext.Request.Body))
                {
                    json = JToken.Parse(reader.ReadToEnd());
                    IEdmTypeReference edmType = GetEdmType(readContext, type);
                    if (!edmType.IsStructured())
                    {
                        throw new ArgumentException("type");
                    }
                    IEdmStructuredTypeReference rootType = edmType.AsStructured();
                    WalkTranslate(json, edmType);
                }

                Stream newPayload = new MemoryStream();
                object result;
                using (StreamWriter writer = new StreamWriter(newPayload, Encoding.UTF8))
                using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jsonWriter, json);
                    jsonWriter.Flush();
                    newPayload.Seek(0, SeekOrigin.Begin);
                    
                    // Dig down into ODataMessageReader's HttpRequestStream and replace with our memory stream.
                    FieldInfo messageField = messageReader.GetType().GetField("message", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    object message = messageField.GetValue(messageReader);
                    FieldInfo requestMessageField = message.GetType().GetField("requestMessage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    object requestMessage = requestMessageField.GetValue(message);
                    FieldInfo streamField = requestMessage.GetType().GetField("_stream", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    streamField.SetValue(requestMessage, newPayload);
                    result = base.Read(messageReader, type, readContext);
                }
                return result;
            }
            else
            {
                return base.Read(messageReader, type, readContext);
            }
        }

        // Walk the JSON body and format instance annotations, and change incoming types based on expected types.
        private void WalkTranslate(JToken node, IEdmTypeReference edmType)
        {
            if (node.Type == JTokenType.Object)
            {
                JObject obj = (JObject)node;
                IEdmStructuredTypeReference structuredType = edmType.AsStructured();
                foreach (JProperty child in node.Children<JProperty>().ToList())
                {
                    IEdmProperty property = structuredType.FindProperty(child.Name);

                    if (child.Name == "odata.type")
                    {
                        obj["@odata.type"] = "#" + obj["odata.type"];
                        obj.Remove("odata.type");
                    }
                    else if (child.Name.Contains("@odata"))
                    {
                        obj[child.Name] = "#" + obj[child.Name];
                    }
                    else if (property != null && 
                        property.Type.TypeKind() == EdmTypeKind.Primitive &&
                        ((IEdmPrimitiveType)property.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.Int64) {
                        obj[child.Name] = Convert.ToInt64(obj[child.Name]);
                    }
                    else if (property != null)
                    {
                        // If type is not IEdmStructuredTypeReference or IEdmCollectionTypeReference, then won't need to convert.
                        IEdmStructuredTypeReference propertyAsStructured = property.Type as IEdmStructuredTypeReference;
                        if (property.Type.TypeKind() == EdmTypeKind.Collection)
                        {
                            WalkTranslate(child.Value, property.Type as IEdmCollectionTypeReference);
                        }
                        else
                        {
                            WalkTranslate(child.Value, property.Type as IEdmStructuredTypeReference);
                        }
                    }
                }
            }
            else if (node.Type == JTokenType.Array)
            {
                IEdmCollectionTypeReference collectionType = (IEdmCollectionTypeReference)edmType;
                
                foreach (JToken child in node.Children().ToList())
                {
                    WalkTranslate(child, collectionType.Definition.AsElementType().ToEdmTypeReference());
                }
            }
        }

        /// <summary>
        /// Retrieves the Edm Type from the given type, using ODataDeserializerContext
        /// </summary>
        /// <param name="context">Context with information about current EdmModel</param>
        /// <param name="type">The type to find an equivalent Edm type to</param>
        /// <returns>Equivalent EdmType to given Type</returns>
        private IEdmTypeReference GetEdmType (ODataDeserializerContext context, Type type)
        {
            if (context.ResourceEdmType != null)
            {
                return context.ResourceEdmType;
            }

            return EdmExtensions.GetExpectedPayloadType(type, context.Path, context.Model);
        }
    }
}
