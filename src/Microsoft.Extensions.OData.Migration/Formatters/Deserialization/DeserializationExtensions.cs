// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Formatters.Deserialization
{
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    internal static class DeserializationExtensions
    {
        /// <summary>
        /// Replace the inner HTTP request stream with substituteStream using reflection
        /// </summary>
        /// <param name="reader">ODataMessageReader which has not read yet</param>
        /// <param name="substituteStream">Replacement stream</param>
        public static void SubstituteRequestStream(this ODataMessageReader reader, Stream substituteStream)
        {
            FieldInfo messageField = reader.GetType().GetField("message", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            object message = messageField.GetValue(reader);
            FieldInfo requestMessageField = message.GetType().GetField("requestMessage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            object requestMessage = requestMessageField.GetValue(message);
            FieldInfo streamField = requestMessage.GetType().GetField("_stream", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            streamField.SetValue(requestMessage, substituteStream);
        }

        // Walk the JSON body and format instance annotations, and change incoming types based on expected types.
        public static void WalkTranslate(this JToken node, IEdmTypeReference edmType)
        {
            if (node == null) return;

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
                        ((IEdmPrimitiveType)property.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.Int64)
                    {
                        obj[child.Name] = Convert.ToInt64(obj[child.Name]);
                    }
                    else if (property != null)
                    {
                        // If type is not IEdmStructuredTypeReference or IEdmCollectionTypeReference, then won't need to convert.
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

    }
}
