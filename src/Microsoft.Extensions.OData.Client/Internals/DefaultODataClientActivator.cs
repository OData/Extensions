//---------------------------------------------------------------------
// <copyright file="DefaultODataClientActivator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Extensions.V3Client
{   
    using System;
    using System.Data.Services.Client;
    using Microsoft.Extensions.Logging;

    internal sealed class DefaultODataClientActivator : IODataV3ClientActivator
    {
        private readonly ILogger<DefaultODataClientActivator> logger;

        /// <summary>
        /// constructor for default OData client activator.
        /// </summary>
        /// <param name="logger">the logger</param>
        public DefaultODataClientActivator(ILogger<DefaultODataClientActivator> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public T CreateClient<T>(Uri serviceRoot) where T : DataServiceContext
        {
            T container = (T)Activator.CreateInstance(typeof(T), new Object[] { serviceRoot });

            Log.ContainerCreated(this.logger, serviceRoot, null);

            return container;
        }
                
        private static class Log
        {
            public static readonly Action<ILogger, Uri, Exception> ContainerCreated = LoggerMessage.Define<Uri>(
                LogLevel.Information,
                new EventId(1003, nameof(ContainerCreated)),
                "Created OData V3 container with root uri:{ServiceRoot}");
        }
    }
}
