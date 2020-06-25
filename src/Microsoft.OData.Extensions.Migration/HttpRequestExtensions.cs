//---------------------------------------------------------------------
// <copyright file="HttpRequestExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Microsoft.OData.Extensions.Migration
{
    internal static class HttpRequestExtensions
    {
        public static bool ContainsV3Headers(this IHeaderDictionary headers)
        {
            return headers.Keys.Any(k => k.ToLower() == "dataserviceversion" || k.ToLower() == "maxdataserviceversion");
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
