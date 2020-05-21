//---------------------------------------------------------------------
// <copyright file="ODataMigrationResourceDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.OData.Extensions.Migration.Formatters.Deserialization
{
    /// <summary>
    /// This customized deserializer for resources will modify request bodies that represent OData entities
    /// to convert OData V3 request body conventions to V4 request body conventions (e.g. quoted longs to longs)
    /// </summary>
    internal class ODataMigrationResourceDeserializer : ODataResourceDeserializer
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
            IEdmTypeReference edmType = GetEdmType(readContext, type);
            if (!edmType.IsStructured())
            {
                throw new ArgumentException("type");
            }

            // Read the entire stream and convert to json
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
                    payload.WalkTranslate(edmType);
                }
            }

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
        /// Retrieves the Edm Type from the given type, using ODataDeserializerContext
        /// </summary>
        /// <param name="context">Context with information about current EdmModel</param>
        /// <param name="type">The type to find an equivalent Edm type to</param>
        /// <returns>Equivalent EdmType to given Type</returns>
        private IEdmTypeReference GetEdmType(ODataDeserializerContext context, Type type)
        {
            if (context.ResourceEdmType != null)
            {
                return context.ResourceEdmType;
            }

            return EdmExtensions.GetExpectedPayloadType(type, context.Path, context.Model);
        }
    }
}
