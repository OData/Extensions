// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using Microsoft.AspNetCore.Http;

    internal static class HttpRequestExtensions
    {
        public static bool ContainsV3Headers(this IHeaderDictionary headers)
        {
            return headers.ContainsKey("dataserviceversion") || headers.ContainsKey("maxdataserviceversion");
        }

        public static void Replace(this IHeaderDictionary headers, string targetHeader, string replacementHeader)
        {
            if (headers.ContainsKey(targetHeader))
            {
                string value = headers[targetHeader];
                headers.Remove(targetHeader);
                headers[replacementHeader] = value;
            }
        }

        public static void SetDefaultContentType(this HttpRequest request, string defaultContentType)
        {
            if (string.IsNullOrEmpty(request.ContentType))
            {
                request.ContentType = "application/json";
            }
        }

        public static bool IsXmlContent(this HttpRequest request)
        {
            return request.ContentType.Contains("text/xml") || request.ContentType.Contains("application/xml");
        }
    }
}
