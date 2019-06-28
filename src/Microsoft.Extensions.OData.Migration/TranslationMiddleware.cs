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
    using System.Threading.Tasks;

    /// <summary>
    /// Translation Middleware currently converts V3 URI to V4 URI (future: converts query, request body as well)
    /// </summary>
    public class TranslationMiddleware
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
        public TranslationMiddleware(RequestDelegate next,
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
            await next(context);
        }

        /// <summary>
        /// Accept a V3 request URI and return a V4 request URI.  V4 request URI retains base path.
        /// </summary>
        /// <param name="requestUri">V3 Request URI</param>
        /// <returns>V4 Request URI</returns>
        public Uri TranslateUri (Uri requestUri)
        {
            // Use UriTranslator to walk v3 segments, translating each to v4 and returning.
            ODataPath v3path = new ODataUriParser(this.v3Model, this.serviceRoot).ParsePath(requestUri);
            UriSegmentTranslator uriTranslator = new UriSegmentTranslator(this.v4Model);
            Microsoft.OData.UriParser.ODataPath v4path = new Microsoft.OData.UriParser.ODataPath(v3path.WalkWith(uriTranslator));
 
            // Create a v4 ODataUri and utilized ODataUriExtensions methods to build v4 URI
            Microsoft.OData.ODataUri v4Uri = new Microsoft.OData.ODataUri()
            {
                Path = v4path,
            };
            Uri v4RelativeUri = Microsoft.OData.ODataUriExtensions.BuildUri(v4Uri, Microsoft.OData.ODataUrlKeyDelimiter.Parentheses);
            Uri v4TranslatedUri = new Uri(this.serviceRoot, v4RelativeUri);

            return v4TranslatedUri;
        }
    }
}
