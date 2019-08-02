﻿// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Formatters.Deserialization
{
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections;
    using System.Runtime.Serialization;

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
        /// Override this method to unquote collection long values 
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
                throw new ArgumentNullException("collectionValue");
            }
            if (elementType == null)
            {
                throw new ArgumentNullException("elementType");
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
