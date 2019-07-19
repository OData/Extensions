﻿// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using System;
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using System.Text;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using System.Linq;
    using ODataPath = AspNet.OData.Routing.ODataPath;
    using System.Runtime.Serialization;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using Newtonsoft.Json;
    using System.Reflection;

    /// <summary>
    /// This customized deserializer handles translating V3 type properties in incoming function/action payloads
    /// </summary>
    /// <param name="provider">ODataDeserializerProvider required by parent class</param>
    public class ODataMigrationActionPayloadDeserializer : ODataActionPayloadDeserializer
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
        public override object Read (ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (readContext.Request.Headers.ContainsKey("DataServiceVersion") || readContext.Request.Headers.ContainsKey("MaxDataServiceVersion"))
            {
                // Collections, Resources, ResourceSet are all passed to their respective Deserializers, except for primitive values.
                // We have to read the JSON body and change those primitive values (like long)
                IEdmAction action = GetAction(readContext);
                JToken payload;
                using (StreamReader reader = new StreamReader(readContext.Request.Body))
                {
                    payload = JToken.Parse(reader.ReadToEnd());
                    foreach (JProperty child in payload.Children<JProperty>().ToList())
                    {
                        string parameterName = child.Name;
                        IEdmOperationParameter parameter = action.Parameters.SingleOrDefault(p => p.Name == parameterName);
                        if (parameter.Type.TypeKind() == EdmTypeKind.Primitive &&
                            ((IEdmPrimitiveType)parameter.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.Int64)
                        {
                            payload[parameterName] = Convert.ToInt64(payload[parameterName]);
                        }
                    }
                }

                // We replace the body of the Http Request with a stream that contains our modified JSON payload.
                Stream newBody = new MemoryStream();
                object result;
                using (StreamWriter writer = new StreamWriter(newBody, Encoding.UTF8))
                using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jsonWriter, payload);
                    jsonWriter.Flush();
                    newBody.Seek(0, SeekOrigin.Begin);

                    // Dig down into ODataMessageReader's HttpRequestStream and replace with our memory stream.
                    FieldInfo messageField = messageReader.GetType().GetField("message", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    object message = messageField.GetValue(messageReader);
                    FieldInfo requestMessageField = message.GetType().GetField("requestMessage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    object requestMessage = requestMessageField.GetValue(message);
                    FieldInfo streamField = requestMessage.GetType().GetField("_stream", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    streamField.SetValue(requestMessage, newBody);
                    result = base.Read(messageReader, type, readContext);
                }

                return result;
            }
            else
            {
                return base.Read(messageReader, type, readContext);
            }
        }

        // Taken from ODataActionPayloadDeserializer base.
        private static IEdmAction GetAction(ODataDeserializerContext readContext)
        {
            if (readContext == null)
            {
                throw new ArgumentNullException("readContext");
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
