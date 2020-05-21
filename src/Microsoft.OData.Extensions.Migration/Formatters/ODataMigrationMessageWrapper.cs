//---------------------------------------------------------------------
// <copyright file="ODataMigrationMessageWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Copy of ODataMessageWrapper for IODataRequestMessage and IODataResponseMessage, because ODataMessageWrapper is required
    /// by formatters, however it is internal to the OData WebApi ASP.NET Core library.
    /// </summary>
    internal class ODataMigrationMessageWrapper : IODataRequestMessage, IODataResponseMessage, IODataPayloadUriConverter, IContainerProvider, IDisposable
    {
        private Stream stream;
        private Dictionary<string, string> headers;
        private IDictionary<string, string> contentIdMapping;
        private static readonly Regex ContentIdReferencePattern = new Regex(@"\$\d", RegexOptions.Compiled);

        public ODataMigrationMessageWrapper()
            : this(stream: null, headers: null)
        {
        }

        public ODataMigrationMessageWrapper(Stream stream)
            : this(stream: stream, headers: null)
        {
        }

        public ODataMigrationMessageWrapper(Stream stream, Dictionary<string, string> headers)
            : this(stream: stream, headers: headers, contentIdMapping: null)
        {
        }

        public ODataMigrationMessageWrapper(Stream stream, Dictionary<string, string> headers, IDictionary<string, string> contentIdMapping)
        {
            this.stream = stream;
            if (headers != null)
            {
                this.headers = headers;
            }
            else
            {
                this.headers = new Dictionary<string, string>();
            }

            this.contentIdMapping = contentIdMapping ?? new Dictionary<string, string>();
        }

        public Stream Stream
        {
            get
            {
                return this.stream;
            }
            set
            {
                this.stream = value;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                return headers;
            }
        }

        public string Method
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public Uri Url
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int StatusCode
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IServiceProvider Container { get; set; }

        // Create a customized message wrapper that ODataInputFormatter will accept
        public static ODataMigrationMessageWrapper Create(Stream stream, IHeaderDictionary headers, IDictionary<string, string> contentIdMapping, IServiceProvider container)
        {
            ODataMigrationMessageWrapper responseMessageWrapper = new ODataMigrationMessageWrapper(
                stream,
                headers.ToDictionary(kvp => kvp.Key, kvp => String.Join(";", kvp.Value)),
                contentIdMapping)
            {
                Container = container
            };

            return responseMessageWrapper;
        }

        public string GetHeader(string headerName)
        {
            string value;
            if (this.headers.TryGetValue(headerName, out value))
            {
                return value;
            }

            return null;
        }

        public Stream GetStream()
        {
            return this.stream;
        }

        public void SetHeader(string headerName, string headerValue)
        {
            this.headers[headerName] = headerValue;
        }

        public Uri ConvertPayloadUri(Uri baseUri, Uri payloadUri)
        {
            if (payloadUri == null)
            {
                throw new ArgumentNullException(nameof(payloadUri));
            }

            string originalPayloadUri = payloadUri.OriginalString;
            if (ContentIdReferencePattern.IsMatch(originalPayloadUri))
            {
                string resolvedUri = ResolveContentId(originalPayloadUri, this.contentIdMapping);
                return new Uri(resolvedUri, UriKind.RelativeOrAbsolute);
            }

            // Returning null for default resolution.
            return null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc/>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.stream != null)
                {
                    this.stream.Dispose();
                }
            }
        }

        private string ResolveContentId(string url, IDictionary<string, string> contentIdToLocationMapping)
        {
            int startIndex = 0;

            while (true)
            {
                startIndex = url.IndexOf('$', startIndex);

                if (startIndex == -1)
                {
                    break;
                }

                int keyLength = 0;

                while (startIndex + keyLength < url.Length - 1 && IsContentIdCharacter(url[startIndex + keyLength + 1]))
                {
                    keyLength++;
                }

                if (keyLength > 0)
                {
                    // Might have matched a $<content-id> alias.
                    string locationKey = url.Substring(startIndex + 1, keyLength);
                    string locationValue;

                    if (contentIdToLocationMapping.TryGetValue(locationKey, out locationValue))
                    {
                        // As location headers MUST be absolute URL's, we can ignore everything 
                        // before the $content-id while resolving it.
                        return locationValue + url.Substring(startIndex + 1 + keyLength);
                    }
                }

                startIndex++;
            }

            return url;
        }

        private static bool IsContentIdCharacter(char c)
        {
            // According to the OData ABNF grammar, Content-IDs follow the scheme.
            // content-id = "Content-ID" ":" OWS 1*unreserved
            // unreserved    = ALPHA / DIGIT / "-" / "." / "_" / "~"
            switch (c)
            {
                case '-':
                case '.':
                case '_':
                case '~':
                    return true;
                default:
                    return Char.IsLetterOrDigit(c);
            }
        }
    }
}