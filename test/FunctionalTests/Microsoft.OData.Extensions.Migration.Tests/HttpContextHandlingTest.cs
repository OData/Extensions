//---------------------------------------------------------------------
// <copyright file="HttpContextHandlingTest.cs" company=".NET Foundation">
//     Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Migration.Tests
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Xunit;
    using Xunit.Abstractions;

    public class HttpContextHandlingTest
    {
        private static readonly Uri serviceRoot = new Uri("http://localhost:80/odata/foobar/baz/");
        private static readonly ODataMigrationMiddleware middleware = TestModelProvider.ODataSvcSampleMiddleware(serviceRoot);
        private readonly ITestOutputHelper output;

        public HttpContextHandlingTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [MemberData(nameof(HttpContextTestData))]
        public void TestHttpContextHandling(string testPathAndQuery, string expectedPathAndQuery)
        {
            HttpContext context = new DefaultHttpContext();
            context.Request.Path = new PathString(testPathAndQuery.Split('?')[0]);
            context.Request.QueryString = new QueryString(testPathAndQuery.Contains("?") ? "?" + testPathAndQuery.Split('?')[1] : "");
            context.Request.Headers["DataServiceVersion"] = "3.0";
            middleware.TranslateV3RequestContext(ref context);
            string result = context.Request.Path.ToString() + (testPathAndQuery.Contains("?") ? context.Request.QueryString.ToString() : "");
            Assert.Equal(expectedPathAndQuery, WebUtility.UrlDecode(result));
        }

        [Fact]
        public void CallingTranslateContextWithInvalidPathBaseThrowsArgumentException ()
        {
            HttpContext context = new DefaultHttpContext();
            // Create a path that doesn't contain service root specified "/odata"
            context.Request.Path = new PathString("/bar/Products");
            Assert.Throws<Data.OData.Query.ODataUnrecognizedPathException>(() =>
            {
                middleware.TranslateV3RequestContext(ref context);
            });
        }

        public static IEnumerable<object[]> HttpContextTestData
        {
            get
            {
                return new List<object[]>()
                {
                    { new object[] { "/Products", "/Products" } }, // Basic path should remain unchanged
                    { new object[] { "/Products?param=hi", "/Products?param=hi" } }, // Basic path with non-OData query should remain unchanged
                    { new object[] { "/Products?$select=Name", "/Products?$select=Name" } }, // Basic path with OData select query should remain unchanged
                    { new object[] { "/Advertisements?$filter=ID ne guid'fbada93e-bad8-47e1-9ea3-17eb294f2cc7'", "/Advertisements?$filter=ID ne fbada93e-bad8-47e1-9ea3-17eb294f2cc7" } } // Path with OData GUID in filter should be translated
                };
            }
        }
    }
}
