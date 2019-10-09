// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Migration.Formatters.Deserialization
{
    using System;
    using System.Text;
    using System.Linq;
    using System.IO;
    using System.Runtime.Serialization;
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using ODataPath = AspNet.OData.Routing.ODataPath;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// This customized deserializer handles translating V3 type properties in incoming function/action payloads.
    /// </summary>
    /// <param name="provider">ODataDeserializerProvider required by parent class</param>
    internal class ODataMigrationActionPayloadDeserializer : ODataActionPayloadDeserializer
    {
        public ODataMigrationActionPayloadDeserializer(ODataDeserializerProvider provider)
            : base(provider)
        {
        }

        /// <summary>
        /// If the incoming request is a V3 request, this Read method extracts the JSON body of the request,
        /// transforms all V3 specific types to V4 compatible types (e.g. quoted longs to longs), and passed on
        /// to base class.
        /// </summary>
        /// <param name="messageReader">Used only by base class</param>
        /// <param name="type">Used only by base class</param>
        /// <param name="readContext">Contains incoming HTTP request and type information</param>
        /// <returns>Deserialized object from request body</returns>
        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            // Collections, Resources, ResourceSet are all passed to their respective Deserializers, except for primitive values.
            // We have to read the JSON body and change those primitive values (like long)
            IEdmAction action = GetAction(readContext);
            JToken payload;
            using (StreamReader reader = new StreamReader(readContext.Request.Body))
            {
                string requestBody = reader.ReadToEnd();
                if (string.IsNullOrEmpty(requestBody))
                {
                    return base.Read(messageReader, type, readContext);
                }
                else
                {
                    payload = JToken.Parse(requestBody);
                    TranslateActionPayload(payload, action);
                }
            }

            // Replace the body of the Http Request with a stream that contains our modified JSON payload.
            Stream substituteStream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(substituteStream, Encoding.UTF8))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, payload);
                jsonWriter.Flush();
                substituteStream.Seek(0, SeekOrigin.Begin);

                messageReader.SubstituteRequestStream(substituteStream);
                return base.Read(messageReader, type, readContext);
            }
        }


        /// <summary>
        /// Action payloads are not Edm types, so they cannot be translated like resources are translated.
        /// To translate the action payload, we walk through each property and compare it to its parameter.  For any properties
        /// that are V3 compatible only, we translate them.  If they are not simple values (e.g., entities or collections), we can
        /// translate them using the same method used for translating resources.
        /// </summary>
        /// <param name="payload">Request body as JSON</param>
        /// <param name="action">Matching OData action information</param>
        private static void TranslateActionPayload(JToken payload, IEdmAction action)
        {
            foreach (JProperty child in payload.Children<JProperty>().ToList())
            {
                string parameterName = child.Name;
                IEdmOperationParameter parameter = action.Parameters.SingleOrDefault(p => p.Name == parameterName);
                if (parameter.Type.TypeKind() == EdmTypeKind.Primitive &&
                    ((IEdmPrimitiveType)parameter.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.Int64)
                {
                    // Translate top level properties
                    payload[parameterName] = Convert.ToInt64(payload[parameterName]);
                }
                else if (parameter.Type.TypeKind() == EdmTypeKind.Entity || parameter.Type.TypeKind() == EdmTypeKind.Collection)
                {
                    // Translate nested resources and collections
                    payload[parameterName].WalkTranslate(parameter.Type);
                }
            }
        }

        // Determine action from readContext
        // This function is copied from default ODataActionPayloadDeserializer because it cannot be inherited
        private static IEdmAction GetAction(ODataDeserializerContext readContext)
        {
            if (readContext == null)
            {
                throw new ArgumentNullException(nameof(readContext));
            }

            ODataPath path = readContext.Path;
            if (path == null || path.Segments.Count == 0)
            {
                throw new SerializationException("OData Path is missing");
            }

            IEdmAction action = null;
            if (path.PathTemplate == "~/unboundaction")
            {
                // only one segment, it may be an unbound action
                OperationImportSegment unboundActionSegment = path.Segments.Last() as OperationImportSegment;
                if (unboundActionSegment != null)
                {
                    IEdmActionImport actionImport = unboundActionSegment.OperationImports.First() as IEdmActionImport;
                    if (actionImport != null)
                    {
                        action = actionImport.Action;
                    }
                }
            }
            else
            {
                // otherwise, it may be a bound action
                OperationSegment actionSegment = path.Segments.Last() as OperationSegment;
                if (actionSegment != null)
                {
                    action = actionSegment.Operations.First() as IEdmAction;
                }
            }

            if (action == null)
            {
                throw new SerializationException("Request is not an action invocation");
            }

            return action;
        }
    }
}
