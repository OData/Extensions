// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// This class was taken from OData v4 Asp Net library 7.1 so that the serializer and deserializer providers
    /// can determine the mapping between an OData type (IEdmTypeReference) and CLR type (Type)
    /// </summary>
    internal class ClrTypeCache
    {
        private ConcurrentDictionary<Type, IEdmTypeReference> _cache =
            new ConcurrentDictionary<Type, IEdmTypeReference>();

        public IEdmTypeReference GetEdmType(Type clrType, IEdmModel model)
        {
            IEdmTypeReference edmType;
            if (!_cache.TryGetValue(clrType, out edmType))
            {
                edmType = model.GetEdmTypeReference(clrType);
                _cache[clrType] = edmType;
            }

            return edmType;
        }
    }
}
