// ------------------------------------------------------------------------------
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

            EdmUtil.IfArgumentNullThrowException(this.v3Model, "v3Model", "V3 model not provided to middleware");
            EdmUtil.IfArgumentNullThrowException(this.v4Model, "v4Model", "V4 model not provided to middleware");
        }

        public async Task InvokeAsync(HttpContext context)
        {

            Console.WriteLine("Headers: " + String.Join(";", context.Request.Headers.Select(pair => pair.Key + "=" + pair.Value).ToArray()));

            // Determine if OData V3
            if (context.Request.Headers["odata-version"] == "3.0")
            {
                // Any need to set the REQUEST headers?  Or just need to modify the response headers?
                Console.WriteLine("Path value: " + context.Request.Path.Value);
                Console.WriteLine("Query string value: " + context.Request.QueryString);

                // Modify response headers
                context.Response.OnStarting(c =>
                {
                    HttpContext httpContext = (HttpContext)c;
                    httpContext.Response.Headers["odata-version"] = new string[] { "3.0;" };
                    httpContext.Response.Headers["dataserviceversion"] = new string[] { "3.0;" };

                    return Task.CompletedTask;
                }, context);
            }

            await next(context);
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
            NameValueCollection queryNvc = HttpUtility.ParseQueryString(requestUri.Query);
            Dictionary<string, string> queryOptions = queryNvc.AllKeys.ToDictionary(k => k.Trim(), k => queryNvc[k].Trim());

            // Create a v4 ODataUri and utilized ODataUriExtensions methods to build v4 URI
            Microsoft.OData.ODataUri v4Uri = new Microsoft.OData.ODataUri()
            {
                Path = v4path,
                Filter = ParseFilterFromQueryOrNull(queryOptions, v4path, v3path)
            };
            Uri v4RelativeUri = Microsoft.OData.ODataUriExtensions.BuildUri(v4Uri, Microsoft.OData.ODataUrlKeyDelimiter.Parentheses);
            Uri v4TranslatedUri = new Uri(this.serviceRoot, v4RelativeUri);

            // Translate Query
            if (queryOptions.ContainsKey("$filter"))
            {
                queryOptions["$filter"] = HttpUtility.ParseQueryString(v4TranslatedUri.Query)["$filter"];
            }
            if (queryOptions.ContainsKey("$inlinecount"))
            {
                queryOptions["$count"] = ParseInlineCountFromQuery(queryOptions["$inlinecount"]);
                queryOptions.Remove("$inlinecount");
            }

            // Join and append query string if applicable
            string v4Query = (queryOptions.Count > 0 ? "?" : "") + String.Join("&", queryOptions.Select(x => x.Key + "=" + x.Value).ToArray());
            v4TranslatedUri = new Uri(Uri.UnescapeDataString(v4TranslatedUri.GetLeftPart(UriPartial.Path) + v4Query));

            return v4TranslatedUri;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pathSegments"></param>
        /// <param name="v3Segments"></param>
        /// <returns></returns>
        private Microsoft.OData.UriParser.FilterClause ParseFilterFromQueryOrNull(Dictionary<string, string> query, Microsoft.OData.UriParser.ODataPath pathSegments, ODataPath v3Segments)
        {
            Microsoft.OData.UriParser.FilterClause v4FilterClause = null;
            if (query.ContainsKey("$filter"))
            {
                // Parse filter clause in v3
                EntitySetSegment entitySegment = v3Segments.Reverse().FirstOrDefault(segment => segment is EntitySetSegment) as EntitySetSegment;
                Data.Edm.IEdmEntityType entityType = entitySegment.EntitySet.ElementType;
                FilterClause v3FilterClause = ODataUriParser.ParseFilter(query["$filter"], v3Model, entityType);

                // Translate node and range variable into v4 format
                QueryNodeTranslator queryTranslator = new QueryNodeTranslator(v4Model);
                Microsoft.OData.UriParser.SingleValueNode v4Node = queryTranslator.VisitNode(v3FilterClause.Expression) as Microsoft.OData.UriParser.SingleValueNode;
                Microsoft.OData.UriParser.RangeVariable v4Var = queryTranslator.TranslateRangeVariable(v3FilterClause.RangeVariable);
                v4FilterClause = new Microsoft.OData.UriParser.FilterClause(v4Node, v4Var);
            }
            return v4FilterClause;
        }

        /// <summary>
        /// Mapping between 
        /// </summary>
        /// <param name="inlineCountOptionValue"></param>
        /// <returns></returns>
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
