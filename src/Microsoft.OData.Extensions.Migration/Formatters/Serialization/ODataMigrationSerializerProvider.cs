// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Migration.Formatters.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.OData;
    using Microsoft.OData.Edm;

    /// <summary>
    /// Implementation of DefaultODataSerializerProvider that hardwires the edm type serializer dispatch process.
    /// The ODataMigrationOutputFormatter creates a customized service provider to pass to this serializer provider
    /// which injects the default serializers for payloads, but for edm types, the function GetEdmTypeSerializer is hardcoded.
    /// 
    /// This is so that the migration serializers are not mixed with other injected serializers, which could lead to behavior that is difficult to debug.
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

        /*// <inheritdoc />
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public override ODataSerializer GetODataPayloadSerializer(Type type, HttpRequest request)
        {
            // Using a Func<IEdmModel> to delay evaluation of the model.
            return GetODataPayloadSerializerImpl(type, () => request.GetModel(), request.ODataFeature().Path, typeof(SerializableError));
        }


        /// <inheritdoc />
        internal ODataSerializer GetODataPayloadSerializerImpl(Type type, Func<IEdmModel> modelFunction, ODataPath path, Type errorType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (modelFunction == null)
            {
                throw new ArgumentNullException(nameof(modelFunction));
            }

            // handle the special types.
            if (type == typeof(ODataServiceDocument))
            {
                return new ODataServiceDocumentSerializer();
            }
            else if (type == typeof(Uri) || type == typeof(ODataEntityReferenceLink))
            {
                return new ODataEntityReferenceLinkSerializer();
            }
            else if (TypeHelper.IsTypeAssignableFrom(typeof(IEnumerable<Uri>), type) || type == typeof(ODataEntityReferenceLinks))
            {
                return new ODataEntityReferenceLinksSerializer();
            }
            else if (type == typeof(ODataError) || type == errorType)
            {
                return new ODataErrorSerializer();
            }
            else if (TypeHelper.IsTypeAssignableFrom(typeof(IEdmModel), type))
            {
                return new ODataMetadataSerializer();
            }

            // Get the model. Using a Func<IEdmModel> to delay evaluation of the model
            // until after the above checks have passed.
            IEdmModel model = modelFunction();

            // if it is not a special type, assume it has a corresponding EdmType.
            ClrTypeCache typeMappingCache = model.GetTypeMappingCache();
            IEdmTypeReference edmType = typeMappingCache.GetEdmType(type, model);

            if (edmType != null)
            {
                bool isCountRequest = path != null && path.Segments.LastOrDefault() is Microsoft.OData.UriParser.CountSegment;
                bool isRawValueRequest = path != null && path.Segments.LastOrDefault() is Microsoft.OData.UriParser.ValueSegment;

                if (((edmType.IsPrimitive() || edmType.IsEnum()) && isRawValueRequest) || isCountRequest)
                {
                    return new ODataRawValueSerializer();
                }
                else
                {
                    return GetEdmTypeSerializer(edmType);
                }
            }
            else
            {
                return null;
            }
        }*/
    }
}
