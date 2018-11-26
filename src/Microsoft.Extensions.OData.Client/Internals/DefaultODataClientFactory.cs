//---------------------------------------------------------------------
// <copyright file="DefaultODataClientFactory.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Extensions.OData.V3Client
{
    using System;
    using System.Data.Services.Client;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Factory for Containers to an ODATA v4 service. 
    /// </summary>
    /// <remarks>
    ///    
    /// For containers to ODATA v3 services, a separate factory will be used. The interfaces will be similar
    /// but it will rely on basic types from the ODATA v3 client package instead of the v4 package.
    /// </remarks>
    internal sealed class DefaultODataClientFactory : IODataV3ClientFactory
    {
        private readonly IOptionsMonitor<ODataV3ClientOptions> options;
        private readonly ILogger<DefaultODataClientFactory> logger;
        private readonly IODataV3ClientActivator activator;

        /// <summary>
        /// constructor for default client factory.
        /// </summary>
        public DefaultODataClientFactory(IODataV3ClientActivator activator, ILogger<DefaultODataClientFactory> logger, IOptionsMonitor<ODataV3ClientOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.activator = activator ?? throw new ArgumentNullException(nameof(activator));
        }

        /// <summary>
        /// create a connection to an OData service, specifying a named HTTP client
        /// </summary>
        /// <param name="name">the logic name of the client to use, including both HttpClient and ODataClient.</param>
        /// <param name="serviceRoot">An absolute URI that identifies the root of a data service.</param>
        /// <returns></returns>
        public T CreateClient<T>(Uri serviceRoot, string name) where T : DataServiceContext
        {
            Log.BeforeCreateClient(this.logger, name, null);

            T container = this.activator.CreateClient<T>(serviceRoot);

            var args = new ClientCreatedArgs(name, container);

            this.OnClientCreated(args);
            return container;
        }

        /// <summary>
        /// call OnProxyCreated for each this.handlers
        /// </summary>
        /// <param name="args">ProxyCreatedArgs</param>
        private void OnClientCreated(ClientCreatedArgs args)
        {
            var op = this.options.Get(args.Name);
            var handlers = op.ODataHandlers;

            foreach (IODataV3ClientHandler handler in handlers)
            {
                Log.OnClientCreatedHandler(this.logger, handler.GetType().FullName, args.Name, null);
                handler.OnClientCreated(args);
            }
        }

        private static class Log
        {
            public static readonly Action<ILogger, string, Exception> BeforeCreateClient = LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(1001, nameof(BeforeCreateClient)),
                "Before Create OData v3 client factory with logical name:{name}");

            public static readonly Action<ILogger, string, string, Exception> OnCreatingClientHandler = LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(1002, nameof(OnCreatingClientHandler)),
                "Calling OnCreatingClient v3 handler {handlerName} with logical name:{name}");

            public static readonly Action<ILogger, string, string, Exception> OnClientCreatedHandler = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(1004, nameof(OnClientCreatedHandler)),
                "Calling OnClientCreated v3 handler {handlerName} with logical name:{name}");
        }
    }
}
