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
        private static readonly Uri serviceRoot = new Uri("http://foo:80/odata/");
        private static readonly TranslationMiddleware middleware = TestModelProvider.ODataSvcSampleMiddleware(serviceRoot);

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
                    { new object[] { "MultipleQueryParametersShouldBeAccountedFor", "Products?$select=Name&$filter=Name eq 'hello'&$expand=Category", "IS_SAME"} },
                    { new object[] { "WhenQueryHasWhitespaceShouldStrip", "Products?$select=Name&$filter=Name     eq 'hello'&$expand=Category", "Products?$select=Name&$filter=Name eq 'hello'&$expand=Category"} },
                    { new object[] { "MultipleQueryParametersRetainOrdering", "Products?$filter=Name eq 'hello'&$select=Name&$expand=Category", "IS_SAME"} }
                };

            }
        }
    }
}
