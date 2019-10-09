// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Migration.Tests
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
        public void TestPathSegmentTranslation(string testQuery, string expectedQuery)
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
                    { new object[] { "Persons", "IS_SAME" } }, // Entity set segment should remain the same

                    // Test KeySegment Properties
                    { new object[] { "Persons(1)", "IS_SAME" } }, // Int key should remain the same
                    { new object[] { "Advertisements(guid'00000000-e90f-4938-b8f6-000000000000')","Advertisements(00000000-e90f-4938-b8f6-000000000000)" } }, // Guid key should be translated
                    
                    // Test Navigation Properties
                    { new object[] { "Products(1)/Categories", "IS_SAME" } }, // Collection navigation property should remain the same
                    { new object[] { "Products(1)/Supplier", "IS_SAME" } }, // Single navigation property should remain the same
                    { new object[] { "Products(12)/Supplier/Location", "IS_SAME" } }, // Single navigation property to simple property should remain the same
                    { new object[] { "Products(12)/Categories(1)/Name", "IS_SAME" } }, // Collection navigation property to simple property should remain the same
                    { new object[] { "Products(22)/ProductDetail/Product/Name", "IS_SAME" } }, // Nested navigation properties should remain the same

                    // Test Bound/Unbound Functions
                    { new object[] { "GetProductsByRating(3)", "ODataDemo.GetProductsByRating(3)" } }, // Unbound function should be same with qualification
                    { new object[] { "Products(1)/Discount", "Products(1)/ODataDemo.Discount" } }, // Bound function shoudl be the same with qualification

                    // Test Simple Properties
                    { new object[] { "Products(1)/Name", "IS_SAME" } }, // Primitive property should be the same
                    { new object[] { "Products(1)/ReleaseDate", "IS_SAME" } }, // DateTime property should be the same
                    { new object[] { "Advertisements(guid'00000000-e90f-4938-b8f6-000000000000')/AirDate", "Advertisements(00000000-e90f-4938-b8f6-000000000000)/AirDate" } }, // Guid key sould be translated
                    { new object[] { "Suppliers(55)/Address", "IS_SAME" } }, // Complex property should be the same
                    { new object[] { "Products(1)/Name/$value", "IS_SAME" } }, // Primitive property value should be the same
                    { new object[] { "$metadata", "IS_SAME"} }, //$ metadata should be the same
                    { new object[] { "$batch", "IS_SAME" } }, // $batch should be the same
                    { new object[] { "Products/$count", "IS_SAME" } } // $count should be the same
                };
            }
        }
    }
}
