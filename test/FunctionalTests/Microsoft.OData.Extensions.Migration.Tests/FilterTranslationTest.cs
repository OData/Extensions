//---------------------------------------------------------------------
// <copyright file="FilterTranslationTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Migration.Tests
{
    using System;
    using System.Net;
    using System.Collections.Generic;
    using Xunit;
    using Xunit.Abstractions;

    public class FilterTranslationTest
    {
        private static readonly Uri serviceRoot = new Uri("http://localhost:80/");
        private static readonly ODataMigrationMiddleware middleware = TestModelProvider.ODataSvcSampleMiddleware(serviceRoot);
        private readonly ITestOutputHelper output;

        public FilterTranslationTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [MemberData(nameof(FilterTranslationTestQueries))]
        public void TestFilterQueryTranslation(string testQuery, string expectedQuery)
        {
            Uri resultUri = middleware.TranslateUri(new Uri(serviceRoot, testQuery));
            Uri expectedUri = new Uri(serviceRoot, expectedQuery == "IS_SAME" ? testQuery : expectedQuery);
            Assert.Equal(expectedUri, resultUri);
        }

        public static IEnumerable<object[]> FilterTranslationTestQueries
        {
            get
            {
                return new List<object[]>()
                {
                    { new object[] { "Products?$filter=3 eq 4", "IS_SAME" } }, // Constant comparison should remain the same
                    { new object[] { "Products?$filter=Name eq 'hello'", "IS_SAME" } }, // eq operator should remain the same
                    { new object[] { "Products?$filter=Name ne 'hello'", "IS_SAME" } }, // ne operator should remain the same
                    { new object[] { "Products?$filter=Name le 'hello' and Name ge 'hello' and Name lt 'hello' and Name gt 'hello'", "IS_SAME" } }, // le, ge, lt & gt operators should remain the same
                    { new object[] { "Advertisements?$filter=ID ne guid'fbada93e-bad8-47e1-9ea3-17eb294f2cc7'", "Advertisements?$filter=ID ne fbada93e-bad8-47e1-9ea3-17eb294f2cc7" } }, // GUID value translation
                    { new object[] { "Advertisements?$filter=AirDate eq datetime'1995-09-01T00:10:00Z'", "Advertisements?$filter=AirDate eq 1995-09-01T00:10:00" } }, // DateTime value translation
                    { new object[] { "Advertisements?$filter=AirDate eq datetime'1995-09-01T00:10:00.0000230-06:00'", "Advertisements?$filter=AirDate eq 1995-09-01T00:10:00"  } }, // DataTime value translation: milliseconds should be stripped off
                    { new object[] { "Products?$filter=startswith(Name,'n') eq true", "IS_SAME" } }, // Canonical function "startswith" should remain the same
                    { new object[] { "Products?$filter=startswith(Name,             'n') eq true", "Products?$filter=startswith(Name,'n') eq true" } }, // the white spaces in a function should be removed
                    { new object[] { "Advertisements?$filter=day(AirDate) eq 3", "IS_SAME" } }, // Canonical function "day" should remain the same
                    { new object[] { "Products?$filter=(4 add 5) mod (4 sub 1) eq 0", "IS_SAME" } }, // Operator precedence grouping should remain the same
                    { new object[] { "Persons?$filter=(isof('ODataDemo.Employee')) and (cast('ODataDemo.Employee')/EmployeeID eq 1099)", "Persons?$filter=isof('ODataDemo.Employee') and cast('ODataDemo.Employee')/EmployeeID eq 1099" } }, // parenthesis around "isof" & "cast" should be removed
                    { new object[] { "Persons?$filter=isof('ODataDemo.Employee')", "IS_SAME" } }, // Canonical function "isof" should remain the same
                    { new object[] { "Products?$filter=Categories/any(c:c/Name eq 'SampleCategory')", "IS_SAME" } }, // Lambda operator "any" should remain the same
                    { new object[] { "Products?$filter=Categories/all(cat:cat/Name eq 'hello')", "IS_SAME" } } // Lambda operator "all" should remain the same
                };
            }
        }
    }
}
