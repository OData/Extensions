﻿// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.OData.Query;
    using Microsoft.Data.OData.Query.SemanticAst;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;

    /// <summary>
    /// Translation Middleware currently converts V3 URI to V4 URI (future: converts query, request body as well)
    /// </summary>
    public class ODataMigrationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly Uri serviceRoot;
        private readonly Data.Edm.IEdmModel v3Model;
        private readonly Microsoft.OData.Edm.IEdmModel v4Model;

        /// <summary>
        /// Constructs an instance of TranslationMiddleware, requiring the root of the service, a V3 model instance and V4 model instance.
        /// </summary>
        /// <param name="next">Delegate required for middleware</param>
        /// <param name="serviceRoot">Base path of service (e.g. "http://foobar:80/baz/")</param>
        /// <param name="v3Model">Instance of V3 EDM model</param>
        /// <param name="v4Model">Instance of V4 EDM model</param>
        public ODataMigrationMiddleware(RequestDelegate next,
                                         Uri serviceRoot,
                                         Data.Edm.IEdmModel v3Model,
                                         Microsoft.OData.Edm.IEdmModel v4Model)
        {
            this.next = next;
            this.serviceRoot = serviceRoot;
            this.v3Model = v3Model;
            this.v4Model = v4Model;

            ExceptionUtil.IfArgumentNullThrowException(this.serviceRoot, "serviceRoot", "Service root (e.g. https://foobar/odata) cannot be null");
            ExceptionUtil.IfArgumentNullThrowException(this.v3Model, "v3Model", "V3 model cannot be null");
            ExceptionUtil.IfArgumentNullThrowException(this.v4Model, "v4Model", "V4 model cannot be null");
        }

        /// <summary>
        /// Middleware method that conditionally modifies an HttpContext to be v4 compatible
        /// </summary>
        /// <param name="context">incoming HttpContext</param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            // If this request is an OData V3 request, translate the URI then pass on
            if (context.Request.Headers["odata-version"] == "3.0")
            {
                TranslateV3RequestContext(ref context);
            }

            await next(context);
        }

        /// <summary>
        /// Changes an HttpContext request path and query from being v3 compatible to being v4 compatible
        /// </summary>
        /// <param name="context">Incoming HttpContext</param>
        public void TranslateV3RequestContext(ref HttpContext context)
        {
            UriBuilder requestBuilder = new UriBuilder(serviceRoot.Scheme, serviceRoot.Host, serviceRoot.Port, context.Request.Path , context.Request.QueryString.Value);
            Uri translatedRequest = TranslateUri(requestBuilder.Uri);
            context.Request.Path = new PathString(translatedRequest.AbsolutePath);
            context.Request.QueryString = new QueryString(translatedRequest.Query);
        }

        /// <summary>
        /// Accept a V3 request URI and return a V4 request URI.  V4 request URI retains base path.
        /// </summary>
        /// <param name="requestUri">V3 Request URI</param>
        /// <returns>V4 Request URI</returns>
        public Uri TranslateUri(Uri requestUri)
        {
            // Use UriTranslator to walk v3 segments, translating each to v4 and returning.
            ODataPath v3path = new ODataUriParser(this.v3Model, this.serviceRoot).ParsePath(requestUri);
            UriSegmentTranslator uriTranslator = new UriSegmentTranslator(this.v4Model);
            Microsoft.OData.UriParser.ODataPath v4path = new Microsoft.OData.UriParser.ODataPath(v3path.WalkWith(uriTranslator));

            // Parse query options for translation
            NameValueCollection requestQuery = HttpUtility.ParseQueryString(requestUri.Query);

            // Create a v4 ODataUri and utilized ODataUriExtensions methods to build v4 URI
            Microsoft.OData.ODataUri v4Uri = new Microsoft.OData.ODataUri()
            {
                Path = v4path,
                Filter = ParseFilterFromQueryOrNull(requestQuery, v4path, v3path)
            };
            Uri v4RelativeUri = Microsoft.OData.ODataUriExtensions.BuildUri(v4Uri, Microsoft.OData.ODataUrlKeyDelimiter.Parentheses);
            Uri v4FullUri = new Uri(serviceRoot, v4RelativeUri);

            // Translated query only contains translated filter clause
            // We need to move everything from v3 query that does not require translation to v4 query
            NameValueCollection translatedQuery = HttpUtility.ParseQueryString(v4FullUri.Query);

            // Copy values from requestQuery to new translated query.  Iterate with multiple loops
            // because NameValueCollections also support multiple values to one key
            foreach (string k in requestQuery)
            {
                foreach (string v in requestQuery.GetValues(k))
                {
                    string key = k.Trim();
                    if (key == "$inlinecount")
                    {
                        translatedQuery["$count"] = ParseInlineCountFromQuery(v.Trim());
                    }
                    else if (key != "$filter")
                    {
                        translatedQuery[k] = v;
                    }
                } 
            }

            UriBuilder builder = new UriBuilder(serviceRoot.Scheme, serviceRoot.Host, serviceRoot.Port, v4FullUri.AbsolutePath);

            // NameValueCollection ToString is overriden to produce a URL-encoded string, decode it to maintain consistency
            builder.Query = WebUtility.UrlDecode(translatedQuery.ToString());

            return builder.Uri;
        }

        // If filter clause is found in query, translate from v3 filter clause to v4 clause
        private Microsoft.OData.UriParser.FilterClause ParseFilterFromQueryOrNull(NameValueCollection query, Microsoft.OData.UriParser.ODataPath pathSegments, ODataPath v3Segments)
        {
            Microsoft.OData.UriParser.FilterClause v4FilterClause = null;
            // The MSDN specification advises checking if NVC contains key by using indexing
            if (query["$filter"] != null)
            {
                // Parse filter clause in v3
                EntitySetSegment entitySegment = v3Segments.Reverse().FirstOrDefault(segment => segment is EntitySetSegment) as EntitySetSegment;
                Data.Edm.IEdmEntityType entityType = entitySegment.EntitySet.ElementType;
                FilterClause v3FilterClause = ODataUriParser.ParseFilter(query["$filter"], v3Model, entityType);

                // Translate node and range variable into v4 format
                QueryNodeTranslator queryTranslator = new QueryNodeTranslator(v4Model);
                Microsoft.OData.UriParser.SingleValueNode v4Node = queryTranslator.Visit((dynamic)v3FilterClause.Expression) as Microsoft.OData.UriParser.SingleValueNode;
                Microsoft.OData.UriParser.RangeVariable v4Var = queryTranslator.TranslateRangeVariable(v3FilterClause.RangeVariable);
                v4FilterClause = new Microsoft.OData.UriParser.FilterClause(v4Node, v4Var);
            }
            return v4FilterClause;
        }

        // Translate allpages -> true, none -> false
        private string ParseInlineCountFromQuery(string inlineCountOptionValue)
        {
            switch (inlineCountOptionValue)
            {
                case "allpages":
                    return "true";
                case "none":
                    return "false";
                default:
                    throw new ArgumentException("Invalid argument for inline count: must be either allpages or none");
            }
        }
    }
}
