//---------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Extensions.V3Client;

namespace Microsoft.OData.Extensions.Client.Tests.Netcore.Handlers
{
    class Startup
    {
        internal IServiceProvider ConfigureServices(ServiceCollection sc)
        {
            sc.AddSingleton<VerificationCounter>();
            sc.AddTransient<VerificationController>();

            sc.AddTransient<VerificationODataClientHandler>();
            sc.AddTransient<VerificationHttpClientHandler>();

            sc
                .AddODataV3Client("Verification")
                .AddODataClientHandler<VerificationODataClientHandler>()
                .AddHttpClient()
                .AddHttpMessageHandler<VerificationHttpClientHandler>();

            return sc.BuildServiceProvider();
        }
    }
}
