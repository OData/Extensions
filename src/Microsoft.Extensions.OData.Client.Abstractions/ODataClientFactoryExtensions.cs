//---------------------------------------------------------------------
// <copyright file="ODataClientFactoryExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Extensions.OData.V3Client
{
    using System;
    using System.Data.Services.Client;

    /// <summary>
    /// Client extensions
    /// </summary>
    public static class ODataV3ClientFactoryExtensions
    {
        /// <summary>
        /// Creates and configures an <see cref="DataServiceContext"/> instance using the configuration that corresponds
        /// to the logical name specified by <see cref="ODataV3ClientOptions.DefaultName"/> with the specified <paramref name="serviceRoot" />
        /// </summary>
        /// <typeparam name="T">The concrate OData client type generated from OData Client code generator.</typeparam>
        /// <param name="factory">The factory used to create the odata client.</param>
        /// <param name="serviceRoot">An absolute URI that identifies the root of a data service.</param>
        /// <returns>A new <see cref="DataServiceContext"/> instance.</returns>
        /// <remarks>
        /// <para>
        /// Each call to <see cref="IODataV3ClientFactory.CreateClient{T}(Uri, string)"/> is guaranteed to return a new <see cref="DataServiceContext"/>
        /// instance. 
        /// </para>
        /// <para>
        /// Callers are also free to mutate the returned <see cref="DataServiceContext"/> instance's public properties
        /// as desired.
        /// </para>
        /// </remarks>
        public static T CreateClient<T>(this IODataV3ClientFactory factory, Uri serviceRoot) where T : DataServiceContext
        {
            return factory.CreateClient<T>(serviceRoot, ODataV3ClientOptions.DefaultName);
        }
    }
}
