﻿//---------------------------------------------------------------------
// <copyright file="DelegatingODataClientHandler.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Extensions.V3Client
{
    using System;
    using System.Data.Services.Client;

    internal sealed class DelegatingODataClientHandler : IODataV3ClientHandler
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
