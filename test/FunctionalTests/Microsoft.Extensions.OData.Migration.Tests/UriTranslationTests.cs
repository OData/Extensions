using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Extensions.OData.Migration.Tests
{
    [TestClass]
    public class UriTranslationTests
    {
        private Uri serviceRoot;
        private TranslationMiddleware middleware;

        [TestInitialize]
        public void Initialize()
        {
            string v4edmx = SampleModelBuilder.TransformODataV3EdmxToODataV4Edmx("V3SampleService.xml", "V2-to-V4-CSDL-NoSap.xsl");
            var v3model = SampleModelBuilder.LoadTestv3Model("V3SampleService.xml");
            var v4model = SampleModelBuilder.LoadTestv4Model(v4edmx);

            // for actual translation, root is irrelevant.  Root is however relevant for determining what part of uri should be translated
            this.serviceRoot = new Uri("http://foo:80/odata");
            this.middleware = new TranslationMiddleware(v3model, v4model, new MigrationOptions()
            {
                ServiceRoot = serviceRoot
            });
        }

        /// TEST TODOS
        /// NavigationPropertyLink tests (need clarification)
        /// BatchReferenceLink tests (need clarification)
        /// Type Segment (need clarification)
        /// Bad models and exception throwing

        #region EntitySet URI Translation Tests
        [TestMethod]
        public void TestEntitySet()
        {
            Assert.AreEqual(serviceRoot.Append("/Persons"), middleware.TranslateUri(serviceRoot.Append("/Persons")));
        }

        [TestMethod]
        public void TestEntitySetNotFoundInV3Model ()
        {
            Assert.ThrowsException <Data.OData.Query.ODataUnrecognizedPathException>(() =>
            {
                middleware.TranslateUri(serviceRoot.Append("/doesntexist"));
            });
        }

        [TestMethod]
        public void TestEntitySet2()
        {
            Assert.AreEqual(serviceRoot.Append("/Products"), middleware.TranslateUri(serviceRoot.Append("/Products")));
        }
        #endregion

        #region NavigationProperty/Link URI Translation Tests
        [TestMethod]
        public void TestCollectionNavigationProperty ()
        {
            Assert.AreEqual(serviceRoot.Append("/Products(1)/Categories"), middleware.TranslateUri(serviceRoot.Append("/Products(1)/Categories")));
        }

        [TestMethod]
        public void TestSingleNavigationProperty ()
        {
            Assert.AreEqual(serviceRoot.Append("/Products(1)/Supplier"), middleware.TranslateUri(serviceRoot.Append("/Products(1)/Supplier")));
        }

        [TestMethod]
        public void TestAccessNavigationThenSimpleProperty()
        {
            Assert.AreEqual(serviceRoot.Append("/Products(12)/Supplier/Location"), middleware.TranslateUri(serviceRoot.Append("/Products(12)/Supplier/Location")));
        }

        [TestMethod]
        public void TestAccessCollectionNavigationThenSimpleProperty()
        {
            Assert.AreEqual(serviceRoot.Append("/Products(12)/Categories(1)/Name"), middleware.TranslateUri(serviceRoot.Append("/Products(12)/Categories(1)/Name")));
        }

        [TestMethod]
        public void TestAccessNestedNavigationProperties()
        {
            Assert.AreEqual(serviceRoot.Append("/Products(1)/ProductDetail/Product/Name"), middleware.TranslateUri(serviceRoot.Append("/Products(1)/ProductDetail/Product/Name")));
        }
        #endregion

        #region KeySegment URI Translation Tests
        [TestMethod]
        public void TestEntityKeyIdentification()
        {
            Assert.AreEqual(serviceRoot.Append("/Persons(1)"), middleware.TranslateUri(serviceRoot.Append("/Persons(1)")));
        }

        [TestMethod]
        public void TestEntityKeyGuidTranslation()
        {
            Assert.AreEqual(serviceRoot.Append("/Advertisements(00000000-e90f-4938-b8f6-000000000000)"), middleware.TranslateUri(serviceRoot.Append("/Advertisements(guid'00000000-e90f-4938-b8f6-000000000000')")));
        }
        #endregion

        [TestMethod]
        public void TestUnboundFunctionImport ()
        {
            // Is the output the correct syntax for v4?
            Assert.AreEqual(serviceRoot.Append("/GetProductsByRating(3)"), middleware.TranslateUri(serviceRoot.Append("/GetProductsByRating(3)")));
        }

        [TestMethod]
        public void TestBoundFunction ()
        {
            // Is the output the correct syntax for v4?
            Assert.AreEqual(serviceRoot.Append("/Products(1)/ODataDemo.Discount"), middleware.TranslateUri(serviceRoot.Append("/Products(1)/Discount")));
        }

        #region Property (including complex, derived) URI Translation Tests
        [TestMethod]
        public void TestEntityPrimitiveProperty ()
        {
            Assert.AreEqual(serviceRoot.Append("/Products(1)/Name"), middleware.TranslateUri(serviceRoot.Append("/Products(1)/Name")));
        }

        [TestMethod]
        public void TestAccessDateTimeProperty()
        {
            Assert.AreEqual(serviceRoot.Append("/Products(12)/ReleaseDate"), middleware.TranslateUri(serviceRoot.Append("/Products(12)/ReleaseDate")));
        }
        
        [TestMethod]
        public void TestAccessDateTimePropertyByGuid()
        {
            Assert.AreEqual(serviceRoot.Append("/Advertisements(00000000-e90f-4938-b8f6-000000000000)/AirDate"), middleware.TranslateUri(serviceRoot.Append("/Advertisements(guid'00000000-e90f-4938-b8f6-000000000000')/AirDate")));
        }


        [TestMethod]
        public void TestEntityComplexTypeProperty()
        {
            Assert.AreEqual(serviceRoot.Append("/Suppliers(55)/Address"), middleware.TranslateUri(serviceRoot.Append("/Suppliers(55)/Address")));
        }

        [TestMethod]
        public void TestEntityPrimitivePropertyValue()
        {
            Assert.AreEqual(serviceRoot.Append("/Products(1)/Name/$value"), middleware.TranslateUri(serviceRoot.Append("/Products(1)/Name/$value")));
        }
        #endregion

        #region Metadata, Batch, and Count (all translations are just returning singleton instances) URI Translation Tests
        [TestMethod]
        public void TestMetadata ()
        {
            Assert.AreEqual(serviceRoot.Append("/$metadata"), middleware.TranslateUri(serviceRoot.Append("/$metadata")));
        }

        [TestMethod]
        public void TestBatch ()
        {
            Assert.AreEqual(serviceRoot.Append("/$batch"), middleware.TranslateUri(serviceRoot.Append("/$batch")));
        }

        [TestMethod]
        public void TestCountEntitySet()
        {
            Assert.AreEqual(serviceRoot.Append("/Products/$count"), middleware.TranslateUri(serviceRoot.Append("/Products/$count")));
        }
        #endregion
    }
}
