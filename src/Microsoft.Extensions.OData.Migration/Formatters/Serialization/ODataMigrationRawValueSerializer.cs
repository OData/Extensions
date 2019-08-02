// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Formatters.Serialization
{
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.OData;
    using System;
    /// <summary>
    /// Converts raw value v3 incompatible types to types that serialize correctly in v3
    /// </summary>
    internal class ODataMigrationRawValueSerializer : ODataRawValueSerializer
    {
        public ODataMigrationRawValueSerializer() : base()
        {
        }

        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (Type.GetTypeCode(type) == TypeCode.UInt64)
            {
                base.WriteObject(graph.ToString(), typeof(string), messageWriter, writeContext);
            }
            else
            {
                base.WriteObject(graph, type, messageWriter, writeContext);
            }
        }
    }
}
