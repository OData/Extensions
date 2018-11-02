//---------------------------------------------------------------------
// <copyright file="VerificationHttpClientHandler.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.OData.Client.Tests.Netcore.Handlers
{
    class VerificationHttpClientHandler : DelegatingHandler
    {
        private readonly VerificationCounter counter;

        public VerificationHttpClientHandler(VerificationCounter counter)
        {
            this.counter = counter;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.counter.HttpInvokeCount++;

            return base.SendAsync(request, cancellationToken);
        }
    }
}
