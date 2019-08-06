// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.


namespace Microsoft.Extensions.OData.Migration.Tests.Mock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc;

    public class CustomersController : ODataController
    {
        private static List<Customer> _customers;

        public CustomersController()
        {
            Generate();
        }

        [EnableQuery]
        public List<Customer> Get()
        {
            return _customers;
        }

        [EnableQuery(AllowedQueryOptions = Microsoft.AspNet.OData.Query.AllowedQueryOptions.SkipToken)]
        public List<Order> GetOrders(int key)
        {
            return _customers[key].Orders;
        }

        [EnableQuery]
        public List<Address> GetAddresses(int key)
        {
            return _customers[key].Addresses;
        }

        [HttpPost]
        public IActionResult Post([FromBody] Customer customer)
        {
            _customers.Add(customer);
            return Created(customer);
        }

        public void Generate()
        {
            _customers = new List<Customer>();
            List<OrderDetail> details = new List<OrderDetail>
                            {
                                new OrderDetail()
                                {
                                    Id = 1,
                                    Name = "1stOrder",
                                },
                                new OrderDetail()
                                {
                                    Id = 2,
                                    Name = "2ndOrder",
                                },
                                new OrderDetail()
                                {
                                    Id = 3,
                                    Name = "3rdOrder"
                                },
                                new OrderDetail()
                                {
                                    Id = 4,
                                    Name = "4thOrder"
                                }
                            };
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Name = "Customer" + i,
                    Order = new Order
                    {
                        Id = i,
                        Name = "Order" + i,
                        Price = i * 100
                    },
                    Address = new Address
                    {
                        Name = "City" + i,
                        Street = "Street" + i,
                    },
                    Token = i % 2 == 0 ? Guid.Parse("d1b6a349-e6d6-4fb2-91c6-8b2eceda85c7") : Guid.Parse("5af3c516-2d3c-4033-95af-07591f18439c"),
                    DateTimeOfBirth = new DateTimeOffset(2000, 1, i, 0, 0, 0, new TimeSpan()),
                    Orders = new List<Order>
                    {
                        new Order
                        {
                            Id = i * 3 - 2,
                            Details = details

                        },
                        new Order
                        {
                            Id = i * 3-1,
                            Details = details
                        },
                        new Order
                        {
                            Id = i * 3,
                            Details = details
                        }
                    },
                    Addresses = new List<Address>()
                    {
                        new Address
                        {
                            Name = "CityA" + i
                        },
                        new Address
                        {
                            Name = "CityB" + i
                        },
                        new Address
                        {
                            Name = "CityC" + i
                        },
                    },
                };
                customer.DynamicProperties["DynamicProperty1"] = 10 - i;
                _customers.Add(customer);
            }
        }
    }

    public class OrdersController : ODataController
    {
        private List<Order> _orders;
        private static List<SpecialOrder> _specialOrders;

        public OrdersController()
        {
            Generate();
        }

        [EnableQuery]
        public List<Order> Get()
        {
            return _orders;
        }

        [EnableQuery]
        public List<Customer> GetCustomers(int key)
        {
            return _orders[key].Customers;
        }

        [EnableQuery]
        public List<SpecialOrder> GetFromSpecialOrder()
        {
            return _specialOrders;
        }

        [HttpPost]
        public IActionResult Post([FromBody] Order order)
        {
            this._orders.Add(order);
            return Created(order);
        }

        [HttpPost]
        public IActionResult SendPointlessNumbers([FromODataUri] int key, ODataActionParameters parameters)
        {
            IEnumerable<long> numbers = (IEnumerable<long>)parameters["Numbers"];
            return Ok();
        }

        public void Generate()
        {
            if (_orders == null)
            {
                _specialOrders = new List<SpecialOrder>();
                _orders = new List<Order>();
                for (int i = 1; i < 10; i++)
                {
                    var order = new Order
                    {
                        Id = i,
                        Name = "Order" + i,
                        Customers = new List<Customer>
                        {
                            new Customer
                            {
                                Id = i,
                                Name = "Customer" + i
                            }
                        },
                        Price = i * 10
                    };

                    _orders.Add(order);
                    var specialOrder = new SpecialOrder
                    {
                        Id = i,
                        SpecialName = "Special Order" + i
                    };

                    _specialOrders.Add(specialOrder);
                }
            }
        }
    }

    public class OrderDetailsController : ODataController
    {
        private List<OrderDetail> _orderDetails;

        public OrderDetailsController ()
        {
            Generate();
        }

        [EnableQuery]
        public List<OrderDetail> Get()
        {
            return _orderDetails;
        }

        [EnableQuery]
        public OrderDetail Get([FromODataUri] int key)
        {
            return _orderDetails.First(detail => detail.Id == key);
        }

        [HttpPost]
        public IActionResult Post([FromBody] OrderDetail detail)
        {
            this._orderDetails.Add(detail);
            return Created(detail);
        }

        [HttpGet]
        public IActionResult GetMaxAmountMax()
        {
            return Ok(this._orderDetails.Max(detail => detail.AmountMax));
        }


        [HttpPost]
        public IActionResult AddToMax([FromODataUri] int key, ODataActionParameters parameters)
        {
            OrderDetail entity;
            try
            {
                entity = this._orderDetails.Single(o => o.Id == key);
                entity.AmountMax += (long)parameters["Amount"];
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        public void Generate()
        {
            if (_orderDetails == null)
            {
                _orderDetails = new List<OrderDetail>();
                for (int i = 1; i < 10; i++)
                {
                    var orderDetail = new OrderDetail
                    {
                        Id = i,
                        Name = "OrderDetail" + i,
                        Amount = i * 100,
                        AmountMax = i * 1000
                    };

                    _orderDetails.Add(orderDetail);
                }
            }
        }
    }
}