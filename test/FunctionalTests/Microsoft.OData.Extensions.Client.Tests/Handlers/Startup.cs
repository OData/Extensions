//---------------------------------------------------------------------
// <copyright file="Startup.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;

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
                .AddODataClient("Verification")
                .AddODataClientHandler<VerificationODataClientHandler>()
                .AddHttpClient()
                .AddHttpMessageHandler<VerificationHttpClientHandler>();

            return sc.BuildServiceProvider();
        }
    }
}
