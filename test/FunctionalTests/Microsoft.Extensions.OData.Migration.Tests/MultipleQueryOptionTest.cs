// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Tests
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class MultipleQueryOptionTest
    {
        private static readonly Uri serviceRoot = new Uri("http://localhost:80/");
        private static readonly ODataMigrationMiddleware middleware = TestModelProvider.ODataSvcSampleMiddleware(serviceRoot);

        [Theory]
        [MemberData(nameof(MultipleQueryOptionTestQueries))]
        public void TestMultipleQueryOptions(string name, string testQuery, string expectedQuery)
        {
            Uri result = middleware.TranslateUri(new Uri(serviceRoot, testQuery));
            Uri expected = new Uri(serviceRoot, expectedQuery == "IS_SAME" ? testQuery : expectedQuery);
            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> MultipleQueryOptionTestQueries
        {
            get
            {
                return new List<object[]>()
                {
                    { new object[] { "MultipleQueryParametersShouldBeAccountedFor", "Products?$select=Name&$filter=Name eq 'hello'&$expand=Category", "Products?$filter=Name eq 'hello'&$select=Name&$expand=Category" } },
                    { new object[] { "WhenQueryHasWhitespaceShouldStrip", "Products?$select=Name&$filter=Name     eq 'hello'&$expand=Category", "Products?$filter=Name eq 'hello'&$select=Name&$expand=Category" } },
                    { new object[] { "QueryWithTopAndSkipStaysTheSame", "Products?$skip=2&$top=2", "IS_SAME" } }
                };

            }
        }
    }
}
