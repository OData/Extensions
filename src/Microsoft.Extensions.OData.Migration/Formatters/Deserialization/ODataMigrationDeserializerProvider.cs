// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Formatters.Deserialization
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.OData.Edm;
    using System;

    public class ODataMigrationDeserializerProvider : DefaultODataDeserializerProvider
    {
        public ODataMigrationDeserializerProvider(IServiceProvider rootContainer)
            : base(rootContainer)
        {
        }

        public override ODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw new ArgumentNullException("edmType");
            }

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Entity:
                case EdmTypeKind.Complex:
                    return new ODataMigrationResourceDeserializer(this);

                case EdmTypeKind.Enum:
                    return new ODataEnumDeserializer();

                case EdmTypeKind.Primitive:
                    return new ODataMigrationPrimitiveDeserializer();

                case EdmTypeKind.Collection:
                    IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                    if (collectionType.ElementType().IsEntity() || collectionType.ElementType().IsComplex())
                    {
                        return new ODataResourceSetDeserializer(this);
                    }
                    else
                    {
                        return new ODataMigrationCollectionDeserializer(this);
                    }

                default:
                    return null;
            }
        }

        public override ODataDeserializer GetODataDeserializer(Type type, HttpRequest request)
        {
            // Using a Func<IEdmModel> to delay evaluation of the model.
            return GetODataDeserializerImpl(type, () => request.GetModel());
        }


        internal ODataDeserializer GetODataDeserializerImpl(Type type, Func<IEdmModel> modelFunction)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (modelFunction == null)
            {
                throw new ArgumentNullException("modelFunction");
            }

            if (type == typeof(Uri))
            {
                return new ODataEntityReferenceLinkDeserializer();
            }

            if (type == typeof(ODataActionParameters) || type == typeof(ODataUntypedActionParameters))
            {
                return new ODataMigrationActionPayloadDeserializer(this);
            }

            // Get the model. Using a Func<IEdmModel> to delay evaluation of the model
            // until after the above checks have passed.
            IEdmModel model = modelFunction();
            ClrTypeCache typeMappingCache = model.GetTypeMappingCache();
            IEdmTypeReference edmType = typeMappingCache.GetEdmType(type, model);

            if (edmType == null)
            {
                return null;
            }
            else
            {
                return GetEdmTypeDeserializer(edmType);
            }
        }
    }
}
