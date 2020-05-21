//---------------------------------------------------------------------
// <copyright file="HttpClientResponseMessage.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Net.Http;
using Microsoft.OData.Extensions.Client.Internals.Handlers;
using Microsoft.OData;
using Microsoft.OData.Client;

namespace Microsoft.OData.Extensions.Client
{
    internal class HttpClientResponseMessage : HttpWebResponseMessage, IODataResponseMessage
    {
        public HttpClientResponseMessage(HttpResponseMessage httpResponse, DataServiceClientConfigurations config)
            : base(httpResponse.ToStringDictionary(),
                  (int)httpResponse.StatusCode,
                  () => { var task = httpResponse.Content.ReadAsStreamAsync(); task.Wait(); return task.Result; })
        {
        }
    }
}
