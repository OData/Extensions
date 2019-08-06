// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Formatters.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;

    /// <summary>
    /// Implementation of DefaultODataSerializerProvider that hardwires in customized migration serializers.
    /// This doesn't use dependency injection so that users have a clean interface instead of injecting multiple
    /// serializers, and also so that this formatter is guaranteed to use only migration serializers.
    /// </summary>
    internal class ODataMigrationSerializerProvider : DefaultODataSerializerProvider
    {
        public ODataMigrationSerializerProvider(IServiceProvider rootContainer)
            : base(rootContainer)
        {
        }

        /// <inheritdoc />
        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw new ArgumentNullException(nameof(edmType));
            }

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Enum:
                    return new ODataEnumSerializer(this);

                case EdmTypeKind.Primitive:
                    return new ODataPrimitiveSerializer();

                case EdmTypeKind.Collection:
                    IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                    if (collectionType.Definition.IsDeltaFeed())
                    {
                        return new ODataDeltaFeedSerializer(this);
                    }
                    else if (collectionType.ElementType().IsEntity() || collectionType.ElementType().IsComplex())
                    {
                        return new ODataMigrationResourceSetSerializer(this);
                    }
                    else
                    {
                        return new ODataMigrationCollectionSerializer(this);
                    }

                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                    return new ODataMigrationResourceSerializer(this);

                default:
                    return null;
            }
        }
    }
}
