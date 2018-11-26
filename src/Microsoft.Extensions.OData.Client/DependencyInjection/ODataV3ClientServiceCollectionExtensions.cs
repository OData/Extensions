//---------------------------------------------------------------------
// <copyright file="ODataV3ClientServiceCollectionExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Extensions.OData.V3Client
{
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Client extensions
    /// </summary>
    public static class ODataV3ClientServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="IODataV3ClientBuilder"/> and related services to the <see cref="IServiceCollection"/> and configures
        /// the default OData client and underlying <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>An <see cref="IODataV3ClientBuilder"/> that can be used to configure the client.</returns>
        /// <remarks>
        /// <para>
        /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using 
        /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
        /// </para>
        /// <para>
        /// Use <see cref="ODataV3ClientOptions.DefaultName"/> as the default name to configure the default client if name not specified.
        /// </para>
        /// </remarks>
        public static IODataV3ClientBuilder AddODataV3Client(this IServiceCollection services)
        {
            return services.AddODataV3Client(ODataV3ClientOptions.DefaultName);
        }

        /// <summary>
        /// Adds the <see cref="IODataV3ClientBuilder"/> and related services to the <see cref="IServiceCollection"/> and configures
        /// a named OData client and underlying <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The logical name of the OData client and <see cref="HttpClient"/> to configure.</param>
        /// <returns>An <see cref="IODataV3ClientBuilder"/> that can be used to configure the client.</returns>
        /// <remarks>
        /// <para>
        /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using 
        /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
        /// </para>
        /// <para>
        /// Use <see cref="ODataV3ClientOptions.DefaultName"/> as the default name to configure the default client if name not specified.
        /// </para>
        /// </remarks>
        public static IODataV3ClientBuilder AddODataV3Client(this IServiceCollection services, string name = ODataV3ClientOptions.DefaultName)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            services.AddLogging();
            services.AddOptions();

            services.TryAddSingleton(typeof(IODataV3ClientFactory), typeof(DefaultODataClientFactory));
            services.TryAddSingleton(typeof(IODataV3ClientActivator), typeof(DefaultODataClientActivator));

            var builder = new DefaultODataClientBuilder(services, name);

            return builder;
        }
    }
}
