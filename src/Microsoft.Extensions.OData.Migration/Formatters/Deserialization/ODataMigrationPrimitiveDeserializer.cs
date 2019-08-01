// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Formatters.Deserialization
{
    using Microsoft.AspNet.OData.Formatter.Deserialization;

    public class ODataMigrationPrimitiveDeserializer : ODataPrimitiveDeserializer
    {
        public ODataMigrationPrimitiveDeserializer()
            : base ()
        {
            // TODO verify that overriding this deserializer is necessary?
        }
    }
}
