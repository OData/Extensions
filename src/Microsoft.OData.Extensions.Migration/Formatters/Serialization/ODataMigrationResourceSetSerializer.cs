// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Migration.Formatters.Serialization
{
    using System;
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.OData;
    using Microsoft.OData.Edm;

    /// <summary>
    /// Converts resource sets to v3 compatible serialized format
    /// Although ODataMigrationResourceSerializer is called through the provider, this resource set serializer
    /// calls ODataMigrationResourceSerializer.WriteObjectInline (not overridden) rather than .WriteObject, 
    /// so there needs to be custom logic in the resource set serializer as well.
    /// </summary>
    public class ODataMigrationResourceSetSerializer : ODataResourceSetSerializer
    {
        public ODataMigrationResourceSetSerializer(ODataSerializerProvider provider)
            : base(provider)
        {
        }

        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw new ArgumentNullException(nameof(messageWriter));
            }

            if (writeContext == null)
            {
                throw new ArgumentNullException(nameof(writeContext));
            }

            IEdmTypeReference resourceSetType = writeContext.GetEdmType(graph, type);

            messageWriter.PreemptivelyTranslateResponseStream(
               resourceSetType,
               (writer) => base.WriteObject(graph, type, writer, writeContext));
        }
    }
}
