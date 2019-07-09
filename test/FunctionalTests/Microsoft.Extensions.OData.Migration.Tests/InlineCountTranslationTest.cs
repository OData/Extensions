// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class InlineCountTranslationTest
    {
        private static readonly Uri serviceRoot = new Uri("http://foo:80/odata/");
        private static readonly ODataMigrationMiddleware middleware = TestModelProvider.ODataSvcSampleMiddleware(serviceRoot);

        [Theory]
        [MemberData(nameof(InlineCountTranslationQueries))]
        public void TestInlineCountTranslations(string name, string testQuery, string expectedQuery)
        {
            Uri result = middleware.TranslateUri(new Uri(serviceRoot, testQuery));
            Uri expected = new Uri(serviceRoot, expectedQuery == "IS_SAME" ? testQuery : expectedQuery);
            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> InlineCountTranslationQueries
        {
            get
            {
                return new List<object[]>()
                {
                    { new object[] { "TestAllPages", "Products?$inlinecount=allpages", "Products?$count=true"} },
                    { new object[] { "TestNonePages", "Products?$inlinecount=none", "Products?$count=false"} },
                    { new object[] { "TestWithRHWhitespace", "Products?$inlinecount= none", "Products?$count=false"} },
                    { new object[] { "TestWithLHWhitespace", "Products?$inlinecount =none", "Products?$count=false"} },
                };
            }
        }
    }
}
