// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.Edm.Csdl;
    using Microsoft.Data.OData.Query;
    using Microsoft.Data.OData.Query.SemanticAst;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Xml;

    /// <summary>
    /// Translation Middleware currently converts V3 URI to V4 URI (future: converts query, request body as well)
    /// </summary>
    public class TranslationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ODataMigrationOptions options;
        private readonly Data.Edm.IEdmModel v3Model;
        private readonly Microsoft.OData.Edm.IEdmModel v4Model;

        /// <summary>
        /// Instantiates a translation middleware.
        /// </summary>
        /// <param name="next">Delegate required by middleware</param>
        /// <param name="options">Contains information necessary for segment translation</param>
        public TranslationMiddleware(RequestDelegate next, ODataMigrationOptions options)
        {
            this.options = options;
            this._next = next;
            /* TODO: on 6/28/2019 I will make ODataMigrationOptions accept IEdmModel for both V3 and V4*/
        }

        /// <summary>
        /// Temporary constructor for unit testing given specific models that are already built (rather than passing in by edmx)
        /// </summary>
        /// <param name="v3Model">V3 model to translate from</param>
        /// <param name="v4Model">V4 model to translate to</param>
        /// <param name="options">Contains information necessary for segment translation</param>
        public TranslationMiddleware(Data.Edm.IEdmModel v3Model, Microsoft.OData.Edm.IEdmModel v4Model, ODataMigrationOptions options)
        {
            // TODO: check if null - is there a Microsoft/approved 3rd party libraries for null checking?
            this.options = options;
            this.v3Model = v3Model;
            this.v4Model = v4Model;
        }

        /// <summary>
        /// Middleware 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
        }

        /// <summary>
        /// Accept a V3 request URI and return a V4 request URI.  V4 request URI retains base path.
        /// </summary>
        /// <param name="requestUri">V3 Request URI</param>
        /// <returns>V4 Request URI</returns>
        public Uri TranslateUri (Uri requestUri)
        {
            // Use UriTranslator to walk v3 segments, translating each to v4 and returning.
            ODataPath v3path = new ODataUriParser(v3Model, options.ServiceRoot).ParsePath(requestUri);
            UriSegmentTranslator uriTranslator = new UriSegmentTranslator(this.v4Model);
            Microsoft.OData.UriParser.ODataPath v4path = new Microsoft.OData.UriParser.ODataPath(v3path.WalkWith(uriTranslator));
 
            // Create a v4 ODataUri and utilized ODataUriExtensions methods to build v4 URI
            Microsoft.OData.ODataUri v4Uri = new Microsoft.OData.ODataUri()
            {
                Path = v4path,
            };
            Uri v4RelativeUri = Microsoft.OData.ODataUriExtensions.BuildUri(v4Uri, Microsoft.OData.ODataUrlKeyDelimiter.Parentheses);
            Uri v4TranslatedUri = new Uri(options.ServiceRoot, v4RelativeUri);

            return v4TranslatedUri;
        }

        
    }
}
