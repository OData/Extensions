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

    public class FilterTranslationTest
    {
        private static readonly Uri serviceRoot = new Uri("http://foo:80/odata/");
        private static readonly ODataMigrationMiddleware middleware = TestModelProvider.ODataSvcSampleMiddleware(serviceRoot);
        private readonly ITestOutputHelper output;

        public FilterTranslationTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [MemberData(nameof(FilterTranslationTestQueries))]
        public void TestFilterQueryTranslation(string name, string testQuery, string expectedQuery)
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
                    { new object[] { "ConstantComparisonShouldRemainSame", "Products?$filter=3 eq 4", "IS_SAME" } },
                    { new object[] { "EqOperatorShouldRemainSame", "Products?$filter=Name eq 'hello'", "IS_SAME" } },
                    { new object[] { "NeOperatorShouldRemainSame", "Products?$filter=Name ne 'hello'", "IS_SAME" } },
                    { new object[] { "LeGeLtGtOperatorsShouldRemainSame", "Products?$filter=Name le 'hello' and Name ge 'hello' and Name lt 'hello' and Name gt 'hello'", "IS_SAME" } },
                    { new object[] { "PropertyEqGuidShouldRemoveGuidName", "Advertisements?$filter=ID ne guid'fbada93e-bad8-47e1-9ea3-17eb294f2cc7'",  "Advertisements?$filter=ID ne fbada93e-bad8-47e1-9ea3-17eb294f2cc7" } },
                    { new object[] { "PropertyEqDatetimeShouldRemoveDatetimeName", "Advertisements?$filter=AirDate eq datetime'1995-09-01T00:10:00Z'", "Advertisements?$filter=AirDate eq 1995-09-01T00:10:00" } },
                    { new object[] { "PropertyEqDatetimeFractionalSecondsShouldRemoveDatetimeName", "Advertisements?$filter=AirDate eq datetime'1995-09-01T00:10:00.0000230-06:00'", "Advertisements?$filter=AirDate eq 1995-09-01T00:10:00"  } },
                    { new object[] { "CanonicalFunctionStartswithShouldRemainSame", "Products?$filter=startswith(Name,'n') eq true", "IS_SAME" } },
                    { new object[] { "ShouldRemoveWhitespace", "Products?$filter=startswith(Name,             'n') eq true", "Products?$filter=startswith(Name,'n') eq true" } },
                    { new object[] { "TypeConversionAsAResultOfCanonicalFunctionShouldRemainSame", "Advertisements?$filter=day(AirDate) eq 3", "IS_SAME" } },
                    { new object[] { "OperatorPrecedenceGroupingShouldRemainSame", "Products?$filter=(4 add 5) mod (4 sub 1) eq 0", "IS_SAME" } },
                    { new object[] { "CastMultiConversionShouldParseToRemoveParen", "Persons?$filter=(isof('ODataDemo.Employee')) and (cast('ODataDemo.Employee')/EmployeeID eq 1099)", "Persons?$filter=isof('ODataDemo.Employee') and cast('ODataDemo.Employee')/EmployeeID eq 1099" } },
                    { new object[] { "CanonicalFunctionIsofShouldRemainSame", "Persons?$filter=isof('ODataDemo.Employee')", "IS_SAME" } },
                    { new object[] { "LambdaOperatorAnyShouldRemainSame", "Products?$filter=Categories/any(c:c/Name eq 'SampleCategory')", "IS_SAME" } },
                    { new object[] { "LambdaOperatorAllShouldRemainSame", "Products?$filter=Categories/all(cat:cat/Name eq 'hello')", "IS_SAME" } }
                };
            }
        }
    }
}
