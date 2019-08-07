// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.


namespace Microsoft.Extensions.OData.Migration.Tests.Mock
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.Extensions.OData.Migration.Tests.Mock;
    using Microsoft.OData.Edm;

    //[Page(PageSize = 2)]
    public class Customer
    {
        public Customer()
        {
            DynamicProperties = new Dictionary<string, object>();
        }
        public int Id { get; set; }

        public string Name { get; set; }

        public Order Order { get; set; }

        public Address Address { get; set; }

        public Guid Token { get; set; }

        public DateTimeOffset DateTimeOfBirth { get; set; }

        [Page(PageSize = 2)]
        public List<Order> Orders { get; set; }

        [Page(PageSize = 1)]
        public List<Address> Addresses { get; set; }

        public Dictionary<string, object> DynamicProperties { get; set; }

    }

    public class Order
    {
        public int Id {
            get; set; }

        public string Name { get; set; }

        public IList<OrderDetail> Details { get; set; }

        public int Price { get; set; }
        //[Page]
        public List<Customer> Customers { get; set; }
    }

    //[Page(PageSize = 2)]
    public class OrderDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public long AmountMax { get; set; }
    }

    public class SpecialOrder : Order
    {
        public string SpecialName { get; set; }
    }

    // [Page(PageSize = 2)]
    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }

    public class MockEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers")
                .EntityType
                .Filter()
                .Expand();

            builder.EntitySet<Order>("Orders")
                .EntityType
                .Filter()
                .Select();

            builder.EntitySet<OrderDetail>("OrderDetails");

            builder.EntityType<OrderDetail>()
                .Collection
                .Function("GetMaxAmountMax")
                .Returns<long>();


            // Test sending long in request and receiving entity back
            var addToMax = builder.EntityType<OrderDetail>()
                .Action("AddToMax");
            addToMax.Parameter<long>("Amount");
            addToMax.ReturnsFromEntitySet<OrderDetail>("OrderDetails");

            // Test sending a collection of primitive long values and receiving nothing
            builder.EntityType<Order>()
                .Action("SendPointlessNumbers")
                .CollectionParameter<long>("Numbers");

            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}