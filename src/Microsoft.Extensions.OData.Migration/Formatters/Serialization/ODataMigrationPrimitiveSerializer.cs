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
    public class ODataMigrationPrimitiveSerializer : ODataPrimitiveSerializer
    {
        public ODataMigrationPrimitiveSerializer()
            : base()
        {
        }

        public override ODataPrimitiveValue CreateODataPrimitiveValue(object graph, IEdmPrimitiveTypeReference primitiveType,
            ODataSerializerContext writeContext)
        {
            if (primitiveType.IsInt64())
            {
                IEdmPrimitiveTypeReference convertedType = (IEdmPrimitiveTypeReference)EdmExtensions.GetEdmPrimitiveTypeOrNull(typeof(string)).ToEdmTypeReference();
                return base.CreateODataPrimitiveValue(graph.ToString(), convertedType, writeContext);
            }
            else
            {
                return base.CreateODataPrimitiveValue(graph, primitiveType, writeContext);
            }
        }
    }
}
