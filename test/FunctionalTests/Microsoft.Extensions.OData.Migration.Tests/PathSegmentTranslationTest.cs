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
        private static readonly Uri serviceRoot = new Uri("http://localhost:80/");
        private static readonly ODataMigrationMiddleware middleware = TestModelProvider.ODataSvcSampleMiddleware(serviceRoot);

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
                    { new object[] { "EntitySetSegmentShouldRemainSame", "Persons", "IS_SAME" } },

                    // Test KeySegment Properties
                    { new object[] { "KeyIdByIntShouldRemainSame", "Persons(1)", "IS_SAME" } },
                    { new object[] { "KeyIdByGuidShouldRemoveGuid", "Advertisements(guid'00000000-e90f-4938-b8f6-000000000000')","Advertisements(00000000-e90f-4938-b8f6-000000000000)" } },
                    
                    // Test Navigation Properties
                    { new object[] { "CollectionNavigationPropertyShouldRemainSame", "Products(1)/Categories", "IS_SAME" } },
                    { new object[] { "SingleNavigationPropertyShouldRemainSame", "Products(1)/Supplier", "IS_SAME" } },
                    { new object[] { "SingleNavigationPropertyToSimplePropertyShouldRemainSame", "Products(12)/Supplier/Location", "IS_SAME" } },
                    { new object[] { "CollectionNavigationPropertyToSimplePropertyShouldRemainSame", "Products(12)/Categories(1)/Name", "IS_SAME" } },
                    { new object[] { "NestedNavigationPropertiesShouldRemainSame", "Products(22)/ProductDetail/Product/Name", "IS_SAME" } },

                    // Test Bound/Unbound Functions
                    { new object[] { "UnboundFunctionShouldBeSameWithQualification", "GetProductsByRating(3)", "ODataDemo.GetProductsByRating(3)" } },
                    { new object[] { "BoundFunctionShoudlBeSameWithQualification", "Products(1)/Discount", "Products(1)/ODataDemo.Discount" } },

                    // Test Simple Properties
                    { new object[] { "PrimitivePropertyShouldBeSame", "Products(1)/Name", "IS_SAME" } },
                    { new object[] { "DatetimePropertyShouldBeSame", "Products(1)/ReleaseDate", "IS_SAME" } },
                    { new object[] { "DatetimePropertyByGuidShouldBeSame", "Advertisements(guid'00000000-e90f-4938-b8f6-000000000000')/AirDate", "Advertisements(00000000-e90f-4938-b8f6-000000000000)/AirDate" } },
                    { new object[] { "ComplexPropertyShouldBeSame", "Suppliers(55)/Address", "IS_SAME" } },
                    { new object[] { "PrimitivePropertyValueShouldBeSame", "Products(1)/Name/$value", "IS_SAME" } },
                    { new object[] { "MetadataShouldBeSame", "$metadata", "IS_SAME"} },
                    { new object[] { "BatchShouldBeSame", "$batch", "IS_SAME" } },
                    { new object[] { "CountShouldBeSame", "Products/$count", "IS_SAME" } }
                };
            }
        }
    }
}
