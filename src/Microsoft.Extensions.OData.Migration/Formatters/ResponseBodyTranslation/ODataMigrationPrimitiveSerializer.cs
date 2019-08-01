namespace Microsoft.Extensions.OData.Migration.Formatters.ResponseBodyTranslation
{
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class ODataMigrationPrimitiveSerializer : ODataPrimitiveSerializer
    {
        public ODataMigrationPrimitiveSerializer()
            : base()
        {
        }

        public override ODataPrimitiveValue CreateODataPrimitiveValue(object graph, IEdmPrimitiveTypeReference primitiveType,
            ODataSerializerContext writeContext)
        {
            Console.WriteLine("CREATING PRIMITIVE VALUE");
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
