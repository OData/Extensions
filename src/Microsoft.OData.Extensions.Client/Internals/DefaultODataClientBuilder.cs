//---------------------------------------------------------------------
// <copyright file="DefaultODataClientBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Client
{
    using Microsoft.Extensions.DependencyInjection;

    internal sealed class DefaultODataClientBuilder : IODataClientBuilder
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
