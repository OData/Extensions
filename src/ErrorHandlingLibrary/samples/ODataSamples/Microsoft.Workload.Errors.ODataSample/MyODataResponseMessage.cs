//---------------------------------------------------------------------
// <copyright file="MyODataResponseMessage.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.OneAPI.ODataSample
{
    using System.Collections.Generic;
    using System.IO;

    using Microsoft.OData;

    class MyODataResponseMessage : IODataResponseMessage
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
