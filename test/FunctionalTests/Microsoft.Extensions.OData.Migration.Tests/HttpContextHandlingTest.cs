﻿// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Tests
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using Xunit;
    using Xunit.Abstractions;

    public class HttpContextHandlingTest
    {
        private static readonly Uri serviceRoot = new Uri("http://foo:80/odata/");
        private static readonly ODataMigrationMiddleware middleware = TestModelProvider.ODataSvcSampleMiddleware(serviceRoot);
        private readonly ITestOutputHelper output;

        public HttpContextHandlingTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [MemberData(nameof(HttpContextTestData))]
        public void TestHttpContextHandling(string name, string testPathAndQuery, string expectedPathAndQuery)
        {
            HttpContext context = new DefaultHttpContext();
            context.Request.Path = new PathString(testPathAndQuery.Split('?')[0]);
            context.Request.QueryString = new QueryString(testPathAndQuery.Contains("?") ? "?" + testPathAndQuery.Split('?')[1] : "");
            context.Request.Headers["odata-service"] = "3.0";
            middleware.TranslateV3RequestContext(ref context);
            string result = Uri.UnescapeDataString(context.Request.Path.ToString() + (testPathAndQuery.Contains("?") ? context.Request.QueryString.ToString() : ""));
            Assert.Equal(result, expectedPathAndQuery);
        }

        [Fact]
        public void CallingTranslateContextWithInvalidPathBaseThrowsArgumentException ()
        {
            HttpContext context = new DefaultHttpContext();
            // Create a path that doesn't contain service root specified "/odata"
            context.Request.Path = new PathString("/Products");
            Assert.Throws<Data.OData.ODataException>(() =>
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
                    { new object[] { "BasicPathShouldRemainUnchanged", "/odata/Products", "/odata/Products" } },
                    { new object[] { "BasicPathWithNonODataQueryShouldRemainUnchanged", "/odata/Products?param=hi", "/odata/Products?param=hi" } },
                    { new object[] { "BasicPathWithODataSelectQueryShouldRemainUnchanged", "/odata/Products?$select=Name", "/odata/Products?$select=Name" } },
                    { new object[] { "PathWithODataGuidInFilterShouldBeChanged", "/odata/Advertisements?$filter=ID ne guid'fbada93e-bad8-47e1-9ea3-17eb294f2cc7'", "/odata/Advertisements?$filter=ID ne fbada93e-bad8-47e1-9ea3-17eb294f2cc7" } }
                };

            }
        }
    }
}