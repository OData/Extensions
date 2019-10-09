// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Tests
{
    using System;
    using System.Net;
    using Xunit;
    using Microsoft.Extensions.OData.Migration.Tests.Mock;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    public class BodyTranslationTest : TestsServiceBase
    {
        private const string CustomersBaseUrl = "{0}/test/Customers";
        private const string OrdersBaseUrl = "{0}/test/Orders";
        private const string OrderDetailsBaseUrl = "{0}/test/OrderDetails";
        private Action<HttpRequestHeaders> AddODataV3Header = (headers) => headers.Add("dataserviceversion", "3.0");
        private Action<HttpRequestHeaders> AddODataV4Header = (headers) => headers.Add("odata-version", "4.0");

        public BodyTranslationTest()
            : base("http://localhost.:8000")
        {
        }

        // TESTED:
        // MaxDataServiceVersion and DataServiceVersion signaling
        // Posting resources as V3 and V4 with appropriate headers
        // Posting resources with wrong headers throws exceptions

        // TODO tests
        // action tests (properties)
        // collections (fails! (select))
        // types

        [Fact]
        public async Task GetRequestsReturnOK()
        {
            HttpResponseMessage orderDetailsResponse = await Get(OrderDetailsBaseUrl);
            HttpResponseMessage ordersResponse = await Get(OrdersBaseUrl);
            HttpResponseMessage customersResponse = await Get(CustomersBaseUrl);
            Assert.Equal(HttpStatusCode.OK, orderDetailsResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, ordersResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, customersResponse.StatusCode);
        }
        
        [Fact]
        public async void GetSimpleResourceSetAsV3HasV3Types()
        {
            HttpResponseMessage response = await Get(OrderDetailsBaseUrl, AddODataV3Header);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Contains(@"""1000""", content); // Check for the long value in the generated OrderDetails
            Assert.Contains("odata.metadata", content);
            Assert.Contains("http://localhost.:8000/test/$metadata#OrderDetails", content);
        }

        [Fact]
        public async void GetSimpleResourceAsV3HasV3Types()
        {
            HttpResponseMessage response = await Get(OrderDetailsBaseUrl + "(1)", AddODataV3Header);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Contains(@"""1000""", content);
            Assert.DoesNotContain(":1000", content);
            Assert.Contains("http://localhost.:8000/test/$metadata#OrderDetails/@Element", content);
        }

        [Fact]
        public async void GetExpandedResourceSetSerializesSuccessfully()
        {
            HttpResponseMessage response = await Get(CustomersBaseUrl + "?$expand=Orders", AddODataV3Header);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Contains(@"Customer1", content);
        }

        [Fact]
        public async void GetMetadata()
        {
            HttpResponseMessage response = await Get("{0}/test/$metadata", AddODataV3Header);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Contains("edmx:DataServices", content);
            Assert.Contains("Version=\"4.0\"", content);
        }

        [Fact]
        public async void GetFilteredResourceSetSerializesSuccessfully()
        {
            HttpResponseMessage response = await Get(OrdersBaseUrl + "?$filter=Price gt 50", AddODataV3Header);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(@"60", content);
            Assert.DoesNotContain(@"50", content);
        }

        [Fact]
        public async void GetInheritedEntities()
        {
            HttpResponseMessage response = await Get(OrdersBaseUrl + "/Microsoft.Extensions.OData.Migration.Tests.Mock.SpecialOrder");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Special Order1", content);
            Assert.DoesNotContain("Microsoft.Extensions.OData.Migration.Tests.Mock.Order", content);
        }

        [Fact]
        public async void GetDatetimeOffsetCanConvertToDateTime()
        {
            HttpResponseMessage response = await Get(CustomersBaseUrl, AddODataV3Header);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject json = (JObject)JToken.Parse(content);
            DateTime customer1BirthDate = DateTime.Parse(json["value"][0]["DateTimeOfBirth"].ToString());
            Assert.Equal("1/1/2000 12:00:00 AM", customer1BirthDate.ToString());
        }

        [Fact]
        public async void CollectionFunctionReturnsLongInV3Type()
        {
            HttpResponseMessage response = await Get(OrderDetailsBaseUrl + "/GetMaxAmountMax", AddODataV3Header);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(@"""9000""", content);
        }

        [Fact]
        public async void ActionWithParameterBodyReturnsV3ResponseBody()
        {
            string body = @"{ ""Amount"": ""100"" }";
            HttpResponseMessage response = await Post(OrderDetailsBaseUrl + "(1)/AddToMax", body, AddODataV3Header);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(@"""1100""", content);
        }

        [Fact]
        public async void ActionWithCollectionCanSendV3PrimitiveTypes()
        {
            string body = @"{ ""Numbers"": [""100"", ""200"", ""300""] }";
            HttpResponseMessage response = await Post(OrdersBaseUrl + "(1)/SendPointlessNumbers", body, AddODataV3Header);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(string.IsNullOrEmpty(content));
        }

        [Fact]
        public async void CanRequestNavigationPropertyofIdentifiedEntity()
        {
            HttpResponseMessage response = await Get(CustomersBaseUrl + "(1)/Orders", AddODataV3Header);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(@"""value"":[{""Id"":4,""Name"":null,""Price"":0},{""Id"":5,""Name"":null,""Price"":0}]", content);
        }

        [Fact]
        public async void SkipAttributeInControllerReturnsNextLink()
        {
            string link = CustomersBaseUrl + "(2)/Addresses";
            HttpResponseMessage response = await Get(link, AddODataV3Header);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(@"""@odata.nextLink"":""" + string.Format(link, BaseAddress) + @"?$skip=1""", content);
        }

        [Fact]
        public async void SkipAndOrderByAreProcessedSuccessfully()
        {
            HttpResponseMessage response = await Get(CustomersBaseUrl + "?$skip=2&$orderby=Name desc", AddODataV3Header);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(OrderDetailsBaseUrl, @"{ ""Id"": 300, ""Name"": ""TestName"", ""Amount"": 3, ""AmountMax"": ""100""}")]
        [InlineData(CustomersBaseUrl, "{\"Id\":1,\"Name\":\"Customer1\",\"Token\":\"5af3d516-2d3c-4033-95af-07591f18439c\",\"DateTimeOfBirth\":\"2000-01-01T00:00:00+03:00\",\"DynamicProperty1\":9,\"Address\":{\"Name\":\"City1\",\"Street\":\"Street1\"},\"Addresses\":[{\"Name\":\"CityA1\",\"Street\":null},{\"Name\":\"CityB1\",\"Street\":null},{\"Name\":\"CityC1\",\"Street\":null}],\"Orders\":[{\"Id\":1,\"Name\":null,\"Price\":0},{\"Id\":2,\"Name\":null,\"Price\":0}] }")]
        public async void PostV3ResourceWithV3TypesAreTranslatedInRequestBody(string url, string body)
        {
            HttpResponseMessage response = await Post(url, body, AddODataV3Header);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Theory]
        [InlineData(OrderDetailsBaseUrl, @"{ ""Id"": 300, ""Name"": ""TestName"", ""Amount"": 3, ""AmountMax"": ""100""}")]
        public async void PostWithDataServiceVersionHeaderReturnsODataV3Metadata(string url, string body)
        {
            HttpResponseMessage response = await Post(url, body, AddODataV3Header);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Contains("odata.metadata", (await response.Content.ReadAsStringAsync()));
            Assert.DoesNotContain("@odata.context", responseContent);
        }

        [Theory]
        [InlineData(OrderDetailsBaseUrl, @"{ ""Id"": 300, ""Name"": ""TestName"", ""Amount"": 3, ""AmountMax"": ""100""}")]
        public async void PostWithMaxDataServiceVersionHeaderReturnsODataV3Metadata(string url, string body)
        {
            HttpResponseMessage response = await Post(url, body, (headers) => headers.Add("maxdataserviceversion", "3.0"));
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Contains("odata.metadata", responseContent);
            Assert.DoesNotContain("@odata.context", responseContent);
        }

        [Theory]
        [InlineData(OrderDetailsBaseUrl, @"{ ""Id"": 300, ""Name"": ""TestName"", ""Amount"": 3, ""AmountMax"": 100}")]
        public async void PostV4DoesNotReturnODataV3Metadata(string url, string body)
        {
            HttpResponseMessage response = await Post(url, body, AddODataV4Header);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.DoesNotContain("odata.metadata", responseContent);
            Assert.Contains("@odata.context", responseContent);
        }

        [Theory]
        [InlineData(OrderDetailsBaseUrl, @"{ ""Id"": 300, ""Name"": ""TestName"", ""Amount"": 3, ""AmountMax"": ""100""}")]
        public async void PostResourceWithQuotedLongWithoutV3HeaderThrowsException(string url, string body)
        {
            HttpResponseMessage response = await Post(url, body);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Theory]
        [InlineData(OrderDetailsBaseUrl, @"{ ""Id"": 300, ""Name"": ""TestName"", ""Amount"": 3, ""AmountMax"": 100}")]
        public async void PostV4ResourceSkipsTranslation(string url, string body)
        {
            HttpResponseMessage response = await Post(url, body, (headers) => headers.Add("odata-version", "4.0"));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        /// <summary>
        /// Execute POST request on the url formatted with the service base address and provided JSON body
        /// A delegate is provided to modify headers (such as specifying OData version)
        /// </summary>
        /// <param name="pathFormat">URL format string e.g., {0}/test/OrderDetails</param>
        /// <param name="body">string body as JSON</param>
        /// <returns>HttpResponseMessage</returns>
        private async Task<HttpResponseMessage> Post(string pathFormat, string body, Action<HttpRequestHeaders> addHeaders = null)
        {
            string uri = string.Format(pathFormat, BaseAddress);
            HttpContent content = new StringContent(body);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return await GetJsonClient(addHeaders).PostAsync(uri, content);
        }

        /// <summary>
        /// Execute a GET request on the url formatted with the base address
        /// </summary>
        /// <param name="pathFormat">URL pattern e.g., {0}/test/OrderDetails</param>
        /// <returns>HttpResponseMessage</returns>
        private async Task<HttpResponseMessage> Get(string pathFormat, Action<HttpRequestHeaders> addHeaders = null)
        {
            string uri = string.Format(pathFormat, BaseAddress);
            return await GetJsonClient(addHeaders).GetAsync(uri);
        }

        /// <summary>
        /// Return an HttpClient configured for accepting JSON responses
        /// </summary>
        /// <returns>HttpClient</returns>
        private static HttpClient GetJsonClient(Action<HttpRequestHeaders> addHeaders = null)
        {
            HttpClient client = new HttpClient();
            addHeaders?.Invoke(client.DefaultRequestHeaders);
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            return client;
        }
    }
}
