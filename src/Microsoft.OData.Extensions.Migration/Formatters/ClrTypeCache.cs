//---------------------------------------------------------------------
// <copyright file="ClrTypeCache.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using Microsoft.OData.Edm;

namespace Microsoft.OData.Extensions.Migration
{
    /// <summary>
    /// This class was taken from OData v4 Asp Net library 7.1 so that the serializer and deserializer providers
    /// can determine the mapping between an OData type (IEdmTypeReference) and CLR type (Type)
    /// </summary>
    internal class ClrTypeCache
    {
        private ConcurrentDictionary<Type, IEdmTypeReference> cache =
            new ConcurrentDictionary<Type, IEdmTypeReference>();

        public IEdmTypeReference GetEdmType(Type clrType, IEdmModel model)
        {
            IEdmTypeReference edmType;
            if (!cache.TryGetValue(clrType, out edmType))
            {
                edmType = model.GetEdmTypeReference(clrType);
                cache[clrType] = edmType;
            }

            return edmType;
        }
    }
}
