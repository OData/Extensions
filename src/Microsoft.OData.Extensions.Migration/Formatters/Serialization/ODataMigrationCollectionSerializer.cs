//---------------------------------------------------------------------
// <copyright file="ODataMigrationCollectionSerializer.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData.Edm;

namespace Microsoft.OData.Extensions.Migration.Formatters.Serialization
{
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
                throw new ArgumentNullException(nameof(messageWriter));
            }

            if (writeContext == null)
            {
                throw new ArgumentNullException(nameof(writeContext));
            }

            IEdmTypeReference collectionType = writeContext.GetEdmType(graph, type);

            // Translate types in response stream according to expected collection type
            messageWriter.PreemptivelyTranslateResponseStream(
               collectionType,
               (writer) => base.WriteObject(graph, type, writer, writeContext));
        }
    }
}
