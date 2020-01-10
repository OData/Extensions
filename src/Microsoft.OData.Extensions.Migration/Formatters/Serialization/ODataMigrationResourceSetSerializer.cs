//---------------------------------------------------------------------
// <copyright file="ODataMigrationResourceSetSerializer.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData.Edm;

namespace Microsoft.OData.Extensions.Migration.Formatters.Serialization
{
    /// <summary>
    /// Converts resource sets to v3 compatible serialized format
    /// Although ODataMigrationResourceSerializer is called through the provider, this resource set serializer
    /// calls ODataMigrationResourceSerializer.WriteObjectInline (not overridden) rather than .WriteObject, 
    /// so there needs to be custom logic in the resource set serializer as well.
    /// </summary>
    public class ODataMigrationResourceSetSerializer : ODataResourceSetSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataMigrationResourceSetSerializer"/> class.
        /// </summary>
        /// <param name="provider">The provider</param>
        public ODataMigrationResourceSetSerializer(ODataSerializerProvider provider)
            : base(provider)
        {
        }

        /// <inheritdoc />
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

            IEdmTypeReference resourceSetType = writeContext.GetEdmType(graph, type);

            messageWriter.PreemptivelyTranslateResponseStream(
               resourceSetType,
               (writer) => base.WriteObject(graph, type, writer, writeContext));
        }
    }
}
