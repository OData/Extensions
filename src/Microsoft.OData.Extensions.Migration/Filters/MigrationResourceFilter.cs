//---------------------------------------------------------------------
// <copyright file="MigrationResourceFilter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.OData.Extensions.Migration.Filters
{
    internal class MigrationResourceFilter : IResourceFilter
    {
        /// <summary>
        /// When inner requests within batch requests are executed, add the appropriate content headers and match the content ID
        /// </summary>
        /// <param name="context">Context for executed resource</param>
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

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
        }
    }
}
