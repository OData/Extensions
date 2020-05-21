//---------------------------------------------------------------------
// <copyright file="ODataMigrationDeserializerProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;

namespace Microsoft.OData.Extensions.Migration.Formatters.Deserialization
{
    /// <summary>
    /// Implementation of DefaultODataDeserializerProvider that hardwires in customized migration deserializers.
    /// This doesn't use dependency injection so that users have a clean interface instead of injecting multiple
    /// deserializers, and also so that this formatter is guaranteed to use only migration deserializers.
    /// </summary>
    internal class ODataMigrationDeserializerProvider : DefaultODataDeserializerProvider
    {
        public ODataMigrationDeserializerProvider(IServiceProvider rootContainer)
            : base(rootContainer)
        {
        }

        /// <summary>
        /// Returns the appropriate deserializer for the given IEdmType.
        /// 
        /// The structure of this method is copied from DefaultODataDeserializerProvider, however the difference is
        /// that the Migration deserializers are hardwired in (use them all or use none of them).
        /// </summary>
        /// <param name="edmType">The edm type to obtain the deserializer for.</param>
        /// <returns>The appropriate deserializer for the given edm type.</returns>
        public override ODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw new ArgumentNullException(nameof(edmType));
            }

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Entity:
                case EdmTypeKind.Complex:
                    // Resources might contain non-V3 compatible types, 
                    return new ODataMigrationResourceDeserializer(this);

                case EdmTypeKind.Enum:
                    return new ODataEnumDeserializer();

                case EdmTypeKind.Primitive:
                    return new ODataPrimitiveDeserializer();

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

        /// <summary>
        /// Gets the appropriate ODataDeserializer for a Type (not necessarily edm type)
        /// 
        /// This method is overidden to call the private GetODataDeserializerImpl in this class.  The reason why it simply
        /// calls GetODataDeserializerImpl is because originally it was in the shared OData library between AspNet and AspNetCore.
        /// Since this extension only caters to OData v4 in AspNetCore, it no longer needs to be shared and can therefore be overridden.
        /// </summary>
        /// <param name="type">Type to get deserializer for.</param>
        /// <param name="request">HttpRequest that contains the IEdmModel</param>
        /// <returns>Appropriate ODataDeserializer for this type.</returns>
        public override ODataDeserializer GetODataDeserializer(Type type, HttpRequest request)
        {
            // Using a Func<IEdmModel> to delay evaluation of the model.
            return GetODataDeserializerImpl(type, () => request.GetModel());
        }

        /// <summary>
        /// Gets the appropriate ODataDeserializer for a Type, converting it into an Edm type if necessary.
        /// 
        /// This method was copied from DefaultODataDeserializerProvider, except instead of returning deserializers via DI,
        /// it directly returns deserializers so that the ActionPayloadDeserializer could be replaced with the ODataMigrationActionPayloadDeserializer,
        /// which is used if the incoming type is a set of parameters for an OData action.
        /// </summary>
        /// <param name="type">Type to get deserializer for.</param>
        /// <param name="modelFunction">Function to get IEdmModel (to delay evaluation of model)</param>
        /// <returns>Appropriate ODataDeserializer for this type.</returns>
        private ODataDeserializer GetODataDeserializerImpl(Type type, Func<IEdmModel> modelFunction)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (modelFunction == null)
            {
                throw new ArgumentNullException(nameof(modelFunction));
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
