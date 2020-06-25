//---------------------------------------------------------------------
// <copyright file="DefaultODataClientFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Client
{
    using System;
    using Microsoft.OData.Client;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Factory for Containers to an ODATA v4 service.
    /// </summary>
    /// <remarks>
    /// For containers to ODATA v3 services, a separate factory will be used. The interfaces will be similar
    /// but it will rely on basic types from the ODATA v3 client package instead of the v4 package.
    /// </remarks>
    internal sealed class DefaultODataClientFactory : IODataClientFactory
    {
        private readonly IOptionsMonitor<ODataClientOptions> options;
        private readonly ILogger<DefaultODataClientFactory> logger;
        private readonly IODataClientActivator activator;

        /// <summary>
        /// constructor for default client factory.
        /// </summary>
        /// <param name="activator">The activator</param>
        /// <param name="logger">The logger</param>
        /// <param name="options">The options</param>
        public DefaultODataClientFactory(IODataClientActivator activator, ILogger<DefaultODataClientFactory> logger, IOptionsMonitor<ODataClientOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.activator = activator ?? throw new ArgumentNullException(nameof(activator));
        }

        /// <summary>
        /// create a connection to an OData service, specifying a named HTTP client
        /// </summary>
        /// <typeparam name="T">The client type.</typeparam>
        /// <param name="serviceRoot">An absolute URI that identifies the root of a data service.</param>
        /// <param name="name">the logic name of the client to use, including both HttpClient and ODataClient.</param>
        /// <returns>The client type instance.</returns>
        public T CreateClient<T>(Uri serviceRoot, string name) where T : DataServiceContext
        {
            // default to highest protocol version client support.
            var odataVersion = ODataProtocolVersion.V401;
            Log.BeforeCreateClient(this.logger, odataVersion, name, null);

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

            foreach (IODataClientHandler handler in handlers)
            {
                var odataVersion = args.ODataClient.MaxProtocolVersion;
                Log.OnClientCreatedHandler(this.logger, odataVersion, handler.GetType().FullName, args.Name, null);
                handler.OnClientCreated(args);
            }
        }

        private static class Log
        {
            public static readonly Action<ILogger, ODataProtocolVersion, string, Exception> BeforeCreateClient = LoggerMessage.Define<ODataProtocolVersion, string>(
                LogLevel.Debug,
                new EventId(1001, nameof(BeforeCreateClient)),
                "Before Create OData {ODataVersion} client factory with logical name:{name}");

            public static readonly Action<ILogger, ODataProtocolVersion, string, string, Exception> OnCreatingClientHandler = LoggerMessage.Define<ODataProtocolVersion, string, string>(
                LogLevel.Information,
                new EventId(1002, nameof(OnCreatingClientHandler)),
                "Calling OnCreatingClient with {ODataVersion} handler {handlerName} with logical name:{name}");

            public static readonly Action<ILogger, ODataProtocolVersion, string, string, Exception> OnClientCreatedHandler = LoggerMessage.Define<ODataProtocolVersion, string, string>(
                LogLevel.Debug,
                new EventId(1004, nameof(OnClientCreatedHandler)),
                "Calling OnClientCreated with {ODataVersion} handler {handlerName} with logical name:{name}");
        }
    }
}
