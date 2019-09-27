//---------------------------------------------------------------------
// <copyright file="DelegatingODataClientHandler.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Client
{
    using System;
    using Microsoft.OData.Client;

    internal sealed class DelegatingODataClientHandler : IODataClientHandler
    {
        private readonly Action<DataServiceContext> action;

        public DelegatingODataClientHandler(Action<DataServiceContext> action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void OnClientCreated(ClientCreatedArgs args)
        {
            this.action(args.ODataClient);
        }
    }
}
