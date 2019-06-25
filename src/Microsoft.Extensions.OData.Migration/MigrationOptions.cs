using System;

namespace Microsoft.Extensions.OData.Migration
{
    /// <summary>
    /// Container for parameters to pass into middleware.
    /// </summary>
    public sealed class MigrationOptions
    {
        // Service root (e.g. the https://foo:80/odata part of https://foo:80/odata/Products) is required for parser
        public Uri ServiceRoot { get; set; }

        // V4 model to validate request
        public Microsoft.OData.Edm.IEdmModel V4Model { get; set; }

        // Path to V3 model edmx; pass in as EDMX because if V3 model is required then 
        // services using this package would need V3 libraries installed.
        public string V3EdmxPath { get; set; }
    }
}
