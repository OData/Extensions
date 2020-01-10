//---------------------------------------------------------------------
// <copyright file="ODataMigrationCollectionDeserializer.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.OData.Edm;

namespace Microsoft.OData.Extensions.Migration.Formatters.Deserialization
{
    /// <summary>
    /// Handles deserializing arrays of non-entities.
    /// </summary>
    internal class ODataMigrationCollectionDeserializer : ODataCollectionDeserializer
    {
        public ODataMigrationCollectionDeserializer(ODataDeserializerProvider provider)
            : base(provider)
        {
        }
     
        /// <summary>
        /// Override this method to translate property values in v3 format to v4 format.
        /// We treat collections (arrays of non-entity/non-complex types) differently from resource sets because
        /// there is an accessible method to override that allows us to examine each value in the collection and convert it if necessary.
        /// </summary>
        /// <param name="collectionValue">Each value in the collection</param>
        /// <param name="elementType">The type of the value</param>
        /// <param name="readContext">Context that contains model, state and HTTP request information</param>
        /// <returns>Deserialized object from request body</returns>
        public override IEnumerable ReadCollectionValue(ODataCollectionValue collectionValue, IEdmTypeReference elementType,
            ODataDeserializerContext readContext)
        {
            if (collectionValue == null)
            {
                throw new ArgumentNullException(nameof(collectionValue));
            }

            if (elementType == null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(elementType);
            if (deserializer == null)
            {
                throw new SerializationException("Type " + elementType.FullName() + " cannot be deserialized");
            }

            foreach (object item in collectionValue.Items)
            {
                if (elementType.IsPrimitive())
                {
                    if (((IEdmPrimitiveType)elementType.Definition).PrimitiveKind == EdmPrimitiveTypeKind.Int64)
                    {
                        yield return Convert.ToInt64(item);
                    }
                    else
                    {
                        yield return item;
                    }
                }
                else
                {
                    yield return deserializer.ReadInline(item, elementType, readContext);
                }
            }
        }
    }
}
