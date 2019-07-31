// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Tests
{
    using System;
    using System.Net;
    using System.Collections.Generic;
    using Xunit;
    using Xunit.Abstractions;
    using Microsoft.AspNetCore.Http;
    using System.IO;
    using Newtonsoft.Json;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.WebUtilities;
    using System.Text;
    using Microsoft.OData;

    public class RequestBodyTranslationTest
    {
        private static readonly Uri serviceRoot = new Uri("http://foo:80/odata/");
        private static readonly Microsoft.OData.Edm.IEdmModel v4model = TestModelProvider.LoadV4ODataSvcModel();
        private static readonly Data.Edm.IEdmModel v3model = TestModelProvider.LoadV3ODataSvcModel();
        private readonly ITestOutputHelper output;

        public RequestBodyTranslationTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [MemberData(nameof(RequestBodyTranslationTestBodies))]
        public void TestRequestBodyTranslation(string name, string url, string body)
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Path = new PathString(url.Split('?')[0]);
            httpContext.Request.QueryString = new QueryString(url.Contains("?") ? "?" + url.Split('?')[1] : "");
            httpContext.Request.Headers["DataServiceVersion"] = "3.0";
            httpContext.Request.ContentType = "application/json;charset=utf-8";
            httpContext.Request.ContentLength = body.Length; // which format? utf-8 chars 1 byte each?
            httpContext.Request.Body = CreateRequestBody(body);

            InputFormatterContext context = new InputFormatterContext(
                httpContext,
                modelName: "ODataDemo.SimpleProduct",
                modelState: new ModelStateDictionary(),
                metadata: new EmptyModelMetadataProvider().GetMetadataForType(typeof(ODataDemo.SimpleProduct)),
                readerFactory: (stream, encoding) => new HttpRequestStreamReader(stream, encoding)
                );

            ODataMigrationInputFormatter formatter = new ODataMigrationInputFormatter(
                new ODataPayloadKind[] {
                    ODataPayloadKind.ResourceSet,
                    ODataPayloadKind.Resource,
                    ODataPayloadKind.Property,
                    ODataPayloadKind.EntityReferenceLink,
                    ODataPayloadKind.EntityReferenceLinks,
                    ODataPayloadKind.Collection,
                    ODataPayloadKind.ServiceDocument,
                    ODataPayloadKind.Error,
                    ODataPayloadKind.Parameter,
                    ODataPayloadKind.Delta
                }
             );

            //Assert.True(formatter.CanRead(context));


        }

        private static Stream CreateRequestBody (string body)
        {
            Stream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, body);
                jsonWriter.Flush();
                stream.Seek(0, SeekOrigin.Begin);
            }
            return stream;
        }

        public static IEnumerable<object[]> RequestBodyTranslationTestBodies
        {
            get
            {
                return new List<object[]>()
                {
                    { new object[] { "RequestBodyWithLongShouldStripQuotes",
                       "/odata/Product", @"{ ID: 1, Name: '', Price: '100'"} },
                };
            }
        }
    }
}
