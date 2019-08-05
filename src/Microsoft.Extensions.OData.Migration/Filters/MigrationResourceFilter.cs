// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Linq;

    internal class MigrationResourceFilter : IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            // Workaround for the 406 Not Acceptable error: strip off "odata=minimalmetadata" from Accept header.
            if (context.HttpContext.Request.Headers.ContainsKey("Accept"))
            {
                var acceptHeader = context.HttpContext.Request.Headers["Accept"];
                if (acceptHeader.Any(x => x.EndsWith(";odata=minimalmetadata")))
                {
                    context.HttpContext.Request.Headers["Accept"] = new StringValues(acceptHeader.Select(x => x.Replace(";odata=minimalmetadata", string.Empty)).ToArray());
                }
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Batch request contexts will be caught at the filter level, so we propagate version 3 headers to those inner requests
            if (context.HttpContext.Request.Headers.ContainsKey("Content-ID"))
            {
                context.HttpContext.Response.Headers["odata-version"] = new string[] { "3.0" };
                context.HttpContext.Response.Headers["dataserviceversion"] = new string[] { "3.0;" };
                context.HttpContext.Response.Headers["Content-ID"] = context.HttpContext.Request.Headers["Content-ID"];
            }
        }
    }
}
