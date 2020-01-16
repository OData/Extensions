//---------------------------------------------------------------------
// <copyright file="MigrationExceptionFilter.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Microsoft.OData.Extensions.Migration.Filters
{
    internal class MigrationExceptionFilter : IExceptionFilter
    {
        private readonly ILogger logger;

        /// <summary>
        /// Catches exceptions and puts them in the response body so that the client receives error messages.
        /// </summary>
        /// <param name="logger">Logger for server-side logging of exceptions</param>
        public MigrationExceptionFilter(ILogger<MigrationExceptionFilter> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// If an exception is thrown and not handled by a controller, handle it and put it in the response body as 500 error
        /// </summary>
        /// <param name="context">Context for exception</param>
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
