//---------------------------------------------------------------------
// <copyright file="HttpClientResponseMessage.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.Extensions.OData.Client.Internals.Handlers;
using Microsoft.OData;
using Microsoft.OData.Client;
using System.Net.Http;

namespace Microsoft.Extensions.OData.Client
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
