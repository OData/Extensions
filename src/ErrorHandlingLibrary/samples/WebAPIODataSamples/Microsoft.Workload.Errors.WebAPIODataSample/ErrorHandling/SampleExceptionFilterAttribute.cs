//---------------------------------------------------------------------
// <copyright file="SampleExceptionFilterAttribute.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors.WebAPIODataSample.ErrorHandling
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Filters;

    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.OData;
    using Microsoft.OData.OneAPI;

    internal class SampleExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            ODataError odataError = null;
            Exception exception = actionExecutedContext.Exception;
            HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError;
            ODataInnerError odataInnerError = null;

            /*
            if (isDevEnvironment)
            {
                odataError.InnerError = new ODataInnerError()
                {
                    StackTrace = exception.StackTrace
                };
            }
            */

            if (exception is AccessDeniedException accessDeniedException)
            {
                httpStatusCode = HttpStatusCode.Forbidden;
                odataError = OneAPIErrorFactory.Create(OneAPIErrors.AccessDenied.Base, odataInnerError);
            }
            else
            {
                odataError = OneAPIErrorFactory.Create(OneAPIErrors.InternalServerError.Base, odataInnerError);
            }

            actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(httpStatusCode, odataError);

            base.OnException(actionExecutedContext);
        }
    }
}