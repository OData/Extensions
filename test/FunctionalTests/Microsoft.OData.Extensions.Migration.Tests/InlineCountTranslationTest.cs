// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Migration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class InlineCountTranslationTest
    {
        private static readonly Uri serviceRoot = new Uri("http://localhost:80/");
        private static readonly ODataMigrationMiddleware middleware = TestModelProvider.ODataSvcSampleMiddleware(serviceRoot);

        [Theory]
        [MemberData(nameof(InlineCountTranslationQueries))]
        public void TestInlineCountTranslations(string testQuery, string expectedQuery)
        {
            Uri result = middleware.TranslateUri(new Uri(serviceRoot, testQuery));
            Uri expected = new Uri(serviceRoot, expectedQuery);
            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> InlineCountTranslationQueries
        {
            get
            {
                return new List<object[]>()
                {
                    { new object[] { "Products?$inlinecount=allpages", "Products?$count=true"} },   // allpages should become true
                    { new object[] { "Products?$inlinecount=none", "Products?$count=false"} },      // none should become false
                    { new object[] { "Products?$inlinecount= none", "Products?$count=false"} },     // white spaces after "=" should be removed
                    { new object[] { "Products?$inlinecount =none", "Products?$count=false"} },     // white spaces before "=" should be removed
                };
            }
        }
    }
}
