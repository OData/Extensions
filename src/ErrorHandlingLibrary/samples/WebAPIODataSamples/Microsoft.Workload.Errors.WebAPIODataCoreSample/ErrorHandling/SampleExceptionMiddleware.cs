//---------------------------------------------------------------------
// <copyright file="SampleExceptionMiddleware.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors.WebAPIODataCoreSample.ErrorHandling
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.OData;
    using Microsoft.OData.OneAPI;

    internal class SampleExceptionMiddleware
    {
        private readonly RequestDelegate next;

        public SampleExceptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ODataError odataError = null;
            HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError;
            ODataInnerError odataInnerError = null;

            /*
            if (isDevEnvironment)
            {
                odataInnerError = new ODataInnerError()
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

            Dictionary<string, string> headers = new Dictionary<string, string>();
            string jsonResponse = ConvertODataErrorToString(httpStatusCode, odataError, headers);

            return context.Response.WriteAsync(jsonResponse);
        }

        private string ConvertODataErrorToString(
            HttpStatusCode statusCode,
            ODataError odataError,
            Dictionary<string, string> headers)
        {
            // Initialize
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage messageToWrite = new MyODataResponseMessage(headers, stream)
            {
                StatusCode = (int)statusCode
            };

            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings { EnableMessageStreamDisposal = false };
            writerSettings.SetContentType(ODataFormat.Json);

            // Write the response payload
            using (var messageWriter = new ODataMessageWriter(messageToWrite, writerSettings))
            {
                messageWriter.WriteError(odataError, odataError.InnerError != null);
            }

            messageToWrite.GetStream().Position = 0;
            string payload = (new StreamReader(stream)).ReadToEnd();
            return payload;
        }

        private class MyODataResponseMessage : IODataResponseMessage
        {
            private readonly Dictionary<string, string> headers;
            private readonly Stream stream;

            public MyODataResponseMessage(Dictionary<string, string> headers, Stream stream)
            {
                this.headers = headers;
                this.stream = stream;
            }

            /// <summary>Gets an enumerable over all the headers for this message.</summary>
            /// <returns>An enumerable over all the headers for this message.</returns>
            public IEnumerable<KeyValuePair<string, string>> Headers { get; }

            /// <summary>Gets or sets the result status code of the response message.</summary>
            /// <returns>The result status code of the response message.</returns>
            public int StatusCode { get; set; }

            /// <summary>Returns a value of an HTTP header.</summary>
            /// <returns>The value of the HTTP header, or null if no such header was present on the message.</returns>
            /// <param name="headerName">The name of the header to get.</param>
            public string GetHeader(string headerName)
            {
                return this.headers.TryGetValue(headerName, out string headerValue) ? headerValue : null;
            }

            /// <summary>Sets the value of an HTTP header.</summary>
            /// <param name="headerName">The name of the header to set.</param>
            /// <param name="headerValue">The value of the HTTP header or 'null' if the header should be removed.</param>
            public void SetHeader(string headerName, string headerValue)
            {
                headers[headerName] = headerValue;
            }

            /// <summary>Gets the stream backing for this message.</summary>
            /// <returns>The stream backing for this message.</returns>
            public Stream GetStream()
            {
                return this.stream;
            }
        }
    }
}