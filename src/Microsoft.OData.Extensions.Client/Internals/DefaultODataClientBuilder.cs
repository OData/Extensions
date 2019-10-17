//---------------------------------------------------------------------
// <copyright file="DefaultODataClientBuilder.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Extensions.V3Client
{
    using Microsoft.Extensions.DependencyInjection;

    internal sealed class DefaultODataClientBuilder : IODataV3ClientBuilder
    {
        public DefaultODataClientBuilder(IServiceCollection services, string name)
        {
            this.Services = services;
            this.Name = name;
        }

        public string Name { get; }

        public IServiceCollection Services { get; }
    }
}
