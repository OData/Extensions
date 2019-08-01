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
    using System.Collections.Generic;
    using System.Text;
    public class ODataMigrationCollectionSerializer : ODataCollectionSerializer
    {
        public ODataMigrationCollectionSerializer(ODataSerializerProvider provider)
            : base(provider)
        {
        }

        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            Console.WriteLine("COLLECTION SERIALIZER");
            if (messageWriter == null)
            {
                throw new ArgumentNullException("messageWriter");
            }

            if (writeContext == null)
            {
                throw new ArgumentNullException("writeContext");
            }

            IEdmTypeReference collectionType = writeContext.GetEdmType(graph, type);

            messageWriter.PreemptivelyTranslateResponseStream(
               collectionType,
               (writer) => base.WriteObject(graph, type, writer, writeContext)
            );
        }
    }
}
