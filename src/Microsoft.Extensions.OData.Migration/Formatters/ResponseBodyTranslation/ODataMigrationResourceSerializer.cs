using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.OData.Migration
{
    public class ODataMigrationResourceSerializer : ODataResourceSerializer
    {
        public ODataMigrationResourceSerializer(ODataSerializerProvider provider)
            : base(provider)
        {
        }

        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            Console.WriteLine("WRITING OBJECT IN RESOURCE SERIALIZER");
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
