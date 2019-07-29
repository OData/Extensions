using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.OData.Migration
{
    public class ODataMigrationResourceSerializer : ODataResourceSerializer
    {
        public ODataMigrationResourceSerializer(ODataSerializerProvider provider)
            : base(provider)
        {
        }

        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            // We don't need to check if v3 because output formatter does that for us
            IEdmTypeReference edmType = GetEdmType(writeContext, graph, type);
            if (!edmType.IsStructured())
            {
                throw new ArgumentException("type");
            }

            Stream substituteStream = new MemoryStream();
            Stream originalStream = messageWriter.SubstituteResponseStream(substituteStream);
            base.WriteObject(graph, type, messageWriter, writeContext);

            // read fake stream, walk translate json object, add item
            JToken responsePayload;
            using (StreamReader reader = new StreamReader(substituteStream))
            {
                responsePayload = JToken.Parse(reader.ReadToEnd());
                WalkTranslate(responsePayload, edmType);
                // add new headers here
            }

            // Write to actual stream
            using (StreamWriter streamWriter = new StreamWriter(originalStream))
            using (JsonTextWriter writer = new JsonTextWriter(streamWriter))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, responsePayload);
                writer.Flush();
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

                    if (property != null &&
                        property.Type.TypeKind() == EdmTypeKind.Primitive &&
                        ((IEdmPrimitiveType)property.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.Int64)
                    {
                        obj[child.Name] = obj[child.Name].ToString();
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

        private IEdmTypeReference GetEdmType(ODataSerializerContext context, object instance, Type type)
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

    }
}
