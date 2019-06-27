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

    public class PathSegmentTranslationTest
    {
        private static readonly Uri serviceRoot = new Uri("http://foo:80/odata/");
        private static readonly TranslationMiddleware middleware = TestModelProvider.ODataSvcSampleMiddleware(serviceRoot);

        [Theory]
        [MemberData(nameof(PathSegmentTranslationQueries))]
        public void TestPathSegmentTranslation(string name, string testQuery, string expectedQuery)
        {
            Uri result = middleware.TranslateUri(new Uri(serviceRoot, testQuery));
            Uri expected = new Uri(serviceRoot, expectedQuery == "IS_SAME" ? testQuery : expectedQuery);
            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> PathSegmentTranslationQueries
        {
            get
            {
                return new List<object[]>()
                {
                     // Test EntitySegment
                    { new object[] { "TestEntitySetSegment", "Persons", "IS_SAME" } },

                    // Test KeySegment Properties
                    { new object[] { "TestKeyIdByInt", "Persons(1)", "IS_SAME" } },
                    { new object[] {  "TestKeyIdByGuid", "Advertisements(guid'00000000-e90f-4938-b8f6-000000000000')","Advertisements(00000000-e90f-4938-b8f6-000000000000)" } },
                    
                    // Test Navigation Properties
                    { new object[] { "TestCollectionNavigationProperty", "Products(1)/Categories", "IS_SAME" } },
                    { new object[] { "TestSingleNavigationProperty", "Products(1)/Supplier", "IS_SAME" } },
                    { new object[] { "TestSingleNavigationPropertyToSimpleProperty", "Products(12)/Supplier/Location", "IS_SAME" } },
                    { new object[] { "TestCollectionNavigationPropertyToSimpleProperty", "Products(12)/Categories(1)/Name", "IS_SAME" } },
                    { new object[] { "TestNestedNavigationProperties", "Products(22)/ProductDetail/Product/Name", "IS_SAME" } },

                    // Test Bound/Unbound Functions
                    { new object[] { "TestUnboundFunction", "GetProductsByRating(3)", "IS_SAME" } },
                    { new object[] { "TestBoundFunction", "Products(1)/Discount", "Products(1)/ODataDemo.Discount" } },

                    // Test Simple Properties
                    { new object[] { "TestPrimitiveProperty", "Products(1)/Name", "IS_SAME" } },
                    { new object[] { "TestDatetimeProperty", "Products(1)/ReleaseDate", "IS_SAME" } },
                    { new object[] { "TestDatetimePropertyByGuid", "Advertisements(guid'00000000-e90f-4938-b8f6-000000000000')/AirDate", "Advertisements(00000000-e90f-4938-b8f6-000000000000)/AirDate" } },
                    { new object[] { "TestComplexProperty", "Suppliers(55)/Address", "IS_SAME" } },
                    { new object[] { "TestPrimitivePropertyValue", "Products(1)/Name/$value", "IS_SAME" } },
                    { new object[] { "TestMetadata", "$metadata", "IS_SAME"} },
                    { new object[] { "TestBatch", "$batch", "IS_SAME" } },
                    { new object[] { "TestCount", "Products/$count", "IS_SAME" } }
                };
            }
        }
    }
}
