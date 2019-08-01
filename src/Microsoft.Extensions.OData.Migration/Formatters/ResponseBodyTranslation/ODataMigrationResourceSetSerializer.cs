// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Formatters.ResponseBodyTranslation
{
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System;
    public class ODataMigrationResourceSetSerializer : ODataResourceSetSerializer
    {
        public ODataMigrationResourceSetSerializer(ODataSerializerProvider provider)
            : base(provider)
        {
        }

        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            Console.WriteLine("RESOURCE SET SERIALIZER CALLED");
            if (messageWriter == null)
            {
                throw new ArgumentNullException("messageWriter");
            }

            if (writeContext == null)
            {
                throw new ArgumentNullException("writeContext");
            }

            IEdmTypeReference resourceSetType = writeContext.GetEdmType(graph, type);

            messageWriter.PreemptivelyTranslateResponseStream(
               resourceSetType,
               (writer) => base.WriteObject(graph, type, writer, writeContext)
            );
        }
    }
}
