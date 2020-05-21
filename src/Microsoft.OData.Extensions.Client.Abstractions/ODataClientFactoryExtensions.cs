//---------------------------------------------------------------------
// <copyright file="ODataClientFactoryExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Client
{
    using System;
    using Microsoft.OData.Client;

    /// <summary>
    /// Client extensions
    /// </summary>
    public static class ODataClientFactoryExtensions
    {
        /// <summary>
        /// Creates and configures an <see cref="DataServiceContext"/> instance using the configuration that corresponds
        /// to the logical name specified by <see cref="ODataClientOptions.DefaultName"/> with the specified <paramref name="serviceRoot" />
        /// </summary>
        /// <typeparam name="T">The concrate OData client type generated from OData Client code generator.</typeparam>
        /// <param name="factory">The factory used to create the odata client.</param>
        /// <param name="serviceRoot">An absolute URI that identifies the root of a data service.</param>
        /// <returns>A new <see cref="DataServiceContext"/> instance.</returns>
        /// <remarks>
        /// <para>
        /// Each call to <see cref="IODataClientFactory.CreateClient{T}(Uri, string)"/> is guaranteed to return a new <see cref="DataServiceContext"/>
        /// instance. 
        /// </para>
        /// <para>
        /// Callers are also free to mutate the returned <see cref="DataServiceContext"/> instance's public properties
        /// as desired.
        /// </para>
        /// </remarks>
        public static T CreateClient<T>(this IODataClientFactory factory, Uri serviceRoot) where T : DataServiceContext
        {
            return factory.CreateClient<T>(serviceRoot, ODataClientOptions.DefaultName);
        }
    }
}
