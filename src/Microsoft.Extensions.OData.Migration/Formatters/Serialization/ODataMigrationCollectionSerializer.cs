// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Formatters.Serialization
{
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System;

    /// <summary>
    /// Customized serializer that converts V3 incompatible types to compatible types in collections
    /// </summary>
    internal class ODataMigrationCollectionSerializer : ODataCollectionSerializer
    {
        public ODataMigrationCollectionSerializer(ODataSerializerProvider provider)
            : base(provider)
        {
        }

        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw new ArgumentNullException("messageWriter");
            }

            if (writeContext == null)
            {
                throw new ArgumentNullException("writeContext");
            }

            IEdmTypeReference collectionType = writeContext.GetEdmType(graph, type);

            // Translate types in response stream according to expected collection type
            messageWriter.PreemptivelyTranslateResponseStream(
               collectionType,
               (writer) => base.WriteObject(graph, type, writer, writeContext)
            );
        }
    }
}
