// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Filters
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using System;

    internal class MigrationExceptionFilter : IExceptionFilter
    {
        private readonly ILogger logger;

        public MigrationExceptionFilter(ILogger<MigrationExceptionFilter> logger)
        {
            this.logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // For the exceptions that are not handled by the controller for a batched request, 
            // return 500 (Interal Server Error) and add the Content-ID header to the response of a batched request.
            if (context.HttpContext.Request.Headers.ContainsKey("Content-ID"))
            {
                context.HttpContext.Response.Headers["Content-ID"] = context.HttpContext.Request.Headers["Content-ID"];
                context.ExceptionHandled = true;
                context.Result = new StatusCodeResult(500);

                logger.LogError(context.Exception, "MigrationExceptionFilter caught an unhandled exception.");
            }
        }
    }
}
