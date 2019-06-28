﻿// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Tests
{
    using System;
    using System.Collections.Generic;
    using Xunit;
    using Xunit.Abstractions;

    public class FilterTranslationTest
    {
        private static readonly Uri serviceRoot = new Uri("http://foo:80/odata/");
        private static readonly TranslationMiddleware middleware = TestModelProvider.ODataSvcSampleMiddleware(serviceRoot);
        private readonly ITestOutputHelper output;

        public FilterTranslationTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [MemberData(nameof(FilterTranslationTestQueries))]
        public void TestFilterQueryTranslation(string name, string testQuery, string expectedQuery)
        {
            Uri result = middleware.TranslateUri(new Uri(serviceRoot, testQuery));
            Uri expected = new Uri(serviceRoot, expectedQuery == "IS_SAME" ? testQuery : expectedQuery);
            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> FilterTranslationTestQueries
        {
            get
            {
                return new List<object[]>()
                {
                    { new object[] { "TestConstantComparison", "Products?$filter=3 eq 4", "IS_SAME" } },
                    { new object[] { "TestEqOperator", "Products?$filter=Name eq 'hello'", "IS_SAME" } },
                    { new object[] { "TestNeOperator", "Products?$filter=Name ne 'hello'", "IS_SAME" } },
                    { new object[] { "TestLeGeLtGtOperators", "Products?$filter=Name le 'hello' and Name ge 'hello' and Name lt 'hello' and Name gt 'hello'", "IS_SAME" } },
                    { new object[] { "TestPropertyEqGuid", "Advertisements?$filter=ID ne guid'fbada93e-bad8-47e1-9ea3-17eb294f2cc7'",  "Advertisements?$filter=ID ne fbada93e-bad8-47e1-9ea3-17eb294f2cc7" } },
                    { new object[] { "TestPropertyEqDatetime", "Advertisements?$filter=AirDate eq datetime'1995-09-01T00:10:00Z'", "Advertisements?$filter=AirDate eq 1995-09-01T00:10:00" } },
                    { new object[] { "TestPropertyEqDatetimeFractionalSeconds", "Advertisements?$filter=AirDate eq datetime'1995-09-01T00:10:00.0000230-06:00'", "Advertisements?$filter=AirDate eq 1995-09-01T00:10:00"  } },
                    { new object[] { "TestCanonicalFunctionStartswith", "Products?$filter=startswith(Name,'n') eq true", "IS_SAME" } },
                    { new object[] { "TestRemovesWhitespace", "Products?$filter=startswith(Name,             'n') eq true", "Products?$filter=startswith(Name,'n') eq true" } },
                    { new object[] { "TestTypeConversionAsAResultOfCanonicalFunction", "Advertisements?$filter=day(AirDate) eq 3", "IS_SAME" } },
                    { new object[] { "TestOperatorPrecedenceGrouping", "Products?$filter=(4 add 5) mod (4 sub 1) eq 0", "IS_SAME" } },
                    { new object[] { "TestCastMultiConversion", "Persons?$filter=(isof('ODataDemo.Employee')) and (cast('ODataDemo.Employee')/EmployeeID eq 1099)", "Persons?$filter=isof('ODataDemo.Employee') and cast('ODataDemo.Employee')/EmployeeID eq 1099" } },
                    { new object[] { "TestCanonicalFunctionIsof", "Persons?$filter=isof('ODataDemo.Employee')", "IS_SAME" } },
                    { new object[] { "TestLambdaOperatorAny", "Products?$filter=Categories/any(c:c/Name eq 'SampleCategory')", "IS_SAME" } },
                    { new object[] { "TestLambdaOperatorAll", "Products?$filter=Categories/all(cat:cat/Name eq 'hello')", "IS_SAME" } }
                };

            }
        }
    }


}