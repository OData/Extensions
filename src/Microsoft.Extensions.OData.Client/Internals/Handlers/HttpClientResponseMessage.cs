//---------------------------------------------------------------------
// <copyright file="HttpClientResponseMessage.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Data.Services.Client;
using System.Net.Http;
using Microsoft.Data.OData;
using Microsoft.Extensions.OData.V3Client.Internals.Handlers;

namespace Microsoft.Extensions.OData.V3Client
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
