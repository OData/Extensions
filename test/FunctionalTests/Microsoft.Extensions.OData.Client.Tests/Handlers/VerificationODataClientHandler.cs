//---------------------------------------------------------------------
// <copyright file="VerificationODataClientHandler.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Extensions.Client.Tests.Netcore.Handlers;
using Microsoft.OData.Extensions.V3Client;
using System;

namespace Microsoft.OData.Extensions.Client.Tests.Netcore
{
    public class VerificationODataClientHandler : IODataV3ClientHandler
    {
        private readonly VerificationCounter counter;

        public VerificationODataClientHandler(VerificationCounter counter)
        {
            this.counter = counter;
        }

        public void OnClientCreated(ClientCreatedArgs args)
        {
            var client = args.ODataClient;
            Console.WriteLine($"MaxProtocolVersion = {client.MaxProtocolVersion}");

            // set header and serialization type
            client.SendingRequest2 += (s, e) =>
            {
                client.Format.UseJson();
                //TODO: uncomment this after properties is supported.
                //e.RequestMessage.SetHeader("api-version", args.ODataClient.Configurations.Properties["api-version"] as string);
            };

            this.counter.ODataInvokeCount ++;
        }
    }
}
