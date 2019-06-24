using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.OData.Migration
{
    public sealed class UriTranslatorOptions
    {
        public Uri ServiceRoot { get; set; }
        public Microsoft.OData.Edm.IEdmModel V4Model { get; set; }
        public string V3EdmxPath { get; set; }
    }
}
