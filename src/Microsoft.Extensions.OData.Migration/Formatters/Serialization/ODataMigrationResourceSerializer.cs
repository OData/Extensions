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
    /// Converts single resources to v3 compatible serialized format
    /// </summary>
    internal class ODataMigrationResourceSerializer : ODataResourceSerializer
    {
        public ODataMigrationResourceSerializer(ODataSerializerProvider provider)
            : base(provider)
        {
        }

        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            // We don't need to check if v3 because output formatter does that for us
            IEdmTypeReference edmType = writeContext.GetEdmType(graph, type);
            if (!edmType.IsStructured())
            {
                throw new ArgumentException("type");
            }

            messageWriter.PreemptivelyTranslateResponseStream(
                edmType,
                (writer) => base.WriteObject(graph, type, writer, writeContext)
            );
        }

        
    }
}
