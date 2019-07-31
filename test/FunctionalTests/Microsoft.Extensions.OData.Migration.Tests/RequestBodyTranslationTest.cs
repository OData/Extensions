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

    public class RequestBodyTranslationTest
    {
        private static readonly Uri serviceRoot = new Uri("http://foo:80/odata/");
        private static readonly ODataMigrationMiddleware middleware = TestModelProvider.ODataSvcSampleMiddleware(serviceRoot);
        private readonly ITestOutputHelper output;

        public RequestBodyTranslationTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [MemberData(nameof(RequestBodyTranslationTestBodies))]
        public void TestRequestBodyTranslation(string name, string testQuery, string expectedQuery)
        {
        }

        public static IEnumerable<object[]> RequestBodyTranslationTestBodies
        {
            get
            {
                return new List<object[]>()
                {
                    { new object[] { "RequestBodyWithLong", "", "" } },
                };
            }
        }
    }
}
