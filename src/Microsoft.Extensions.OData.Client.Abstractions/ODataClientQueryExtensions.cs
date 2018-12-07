//---------------------------------------------------------------------
// <copyright file="ODataClientQueryExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.OData.Client
{
    /// <summary>
    /// OData Client query extensions
    /// </summary>
    public static class ODataClientQueryExtensions
    {
        /// <summary>
        /// convert the queryable to DataServiceQuery and execute it.
        /// </summary>
        /// <typeparam name="TElement">the entity type.</typeparam>
        /// <param name="queryable">the OData querable.</param>
        /// <returns>the OData query result.</returns>
        public static async Task<IEnumerable<TElement>> ExecuteAsync<TElement>(this IQueryable queryable)
        {
            var collection = (DataServiceQuery<TElement>)queryable;
            return await collection.ExecuteAsync();
        }
    }
}
