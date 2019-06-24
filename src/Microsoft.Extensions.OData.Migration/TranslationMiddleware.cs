using Microsoft.AspNetCore.Http;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using System;
using System.Threading.Tasks;
using System.Xml;

namespace Microsoft.Extensions.OData.Migration
{
    /// <summary>
    /// Translation Middleware currently converts V3 URI to V4 URI (future: converts query, request body as well)
    /// </summary>
    public class TranslationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly UriTranslatorOptions options;
        private readonly Data.Edm.IEdmModel v3Model;
        private readonly Microsoft.OData.Edm.IEdmModel v4Model;

        /// <summary>
        /// Instantiates a translation middleware.
        /// </summary>
        /// <param name="next">Delegate required by middleware</param>
        /// <param name="options">Contains information necessary for segment translation</param>
        public TranslationMiddleware(RequestDelegate next, UriTranslatorOptions options)
        {
            this.options = options;
            this._next = next;

            using (XmlReader reader = XmlReader.Create(options.V3EdmxPath))
            {
                this.v3Model = EdmxReader.Parse(reader);
            }
            this.v4Model = options.V4Model;
        }

        /// <summary>
        /// Temporary constructor for unit testing given specific models that are already built (rather than passing in by edmx)
        /// </summary>
        /// <param name="v3Model">V3 model to translate from</param>
        /// <param name="v4Model">V4 model to translate to</param>
        /// <param name="options">Contains information necessary for segment translation</param>
        public TranslationMiddleware(Data.Edm.IEdmModel v3Model, Microsoft.OData.Edm.IEdmModel v4Model, UriTranslatorOptions options)
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
            // When query is complete, this method will be implemented to take incoming URI and translate to v4 URI
            await _next(context);
        }

        /// <summary>
        /// Accept a V3 request URI and return a V4 request URI.  V4 request URI retains base path.
        /// </summary>
        /// <param name="requestUri">V3 Request URI</param>
        /// <returns>V4 Request URI</returns>
        public Uri TranslateUri (Uri requestUri)
        {
            ODataPath v3path = (new ODataUriParser(v3Model, options.ServiceRoot)).ParsePath(requestUri);

            // Use UriTranslator to walk v3 segments, translating each to v4 and returning.  The order of IEnumerable is guaranteed
            // because it is implemented as an IList underneath.
            UriTranslator uriTranslator = new UriTranslator(this.v4Model);
            Microsoft.OData.UriParser.ODataPath v4path = new Microsoft.OData.UriParser.ODataPath(v3path.WalkWith(uriTranslator));

            /* CODE COMMENTED because it's part of the query parsing
            NameValueCollection queryCollection = HttpUtility.ParseQueryString(requestUri.Query);
            StringBuilder v4QueryEnd = new StringBuilder();
            Microsoft.OData.UriParser.FilterClause filter = null;
            Dictionary<string, string> queryOptions = new Dictionary<string, string>();
            foreach (var k in queryCollection.AllKeys)
            {
                if (k == "$filter")
                {
                    filter = TranslateFilterClause(queryCollection[k], v4path, v3path);
                }
            }*/

            // Create a v4 ODataUri and utilized ODataUriExtensions methods to build v4 URI
            Microsoft.OData.ODataUri v4Uri = new Microsoft.OData.ODataUri()
            {
                Path = v4path,
                //Filter = filter
            };
            Uri builtV4Uri = Microsoft.OData.ODataUriExtensions.BuildUri(v4Uri, Microsoft.OData.ODataUrlKeyDelimiter.Slash);

            return new Uri(options.ServiceRoot.ToString() + "/" + builtV4Uri);
        }

        /* CODE COMMENTED FOR THIS PULL REQUEST because it's part of the query parsing
        public Microsoft.OData.UriParser.FilterClause TranslateFilterClause (string clause, Microsoft.OData.UriParser.ODataPath pathSegments, ODataPath v3Segments)
        {
            EntitySetSegment entitySegment = v3Segments.Reverse().FirstOrDefault(segment => segment is EntitySetSegment) as EntitySetSegment;
            Data.Edm.IEdmEntityType entityType = entitySegment.EntitySet.ElementType;
            FilterClause v3FilterClause = Data.OData.Query.ODataUriParser.ParseFilter(clause, v3Model, entityType);
            // into v4 format
            QueryTranslator queryTranslator = new QueryTranslator(v4Model);
            Microsoft.OData.UriParser.SingleValueNode v4Node = queryTranslator.VisitNode(v3FilterClause.Expression) as Microsoft.OData.UriParser.SingleValueNode;
            Console.WriteLine("Past node translation");
            Microsoft.OData.UriParser.RangeVariable v4Var = queryTranslator.TranslateRangeVariable(v3FilterClause.RangeVariable);
            Console.WriteLine("Past var translation");
            Microsoft.OData.UriParser.FilterClause v4FilterClause = new Microsoft.OData.UriParser.FilterClause(v4Node, v4Var);
            return v4FilterClause;
        }*/
    }
}
