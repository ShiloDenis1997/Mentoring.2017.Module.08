using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using LinqToDB.Data;
using Northwind.Model.LinqToDb;
using Northwind.Model.LinqToDb.Entities;
using NUnit.Framework;

namespace Queries.Demonstration
{
    [TestFixture]
    public class Task3
    {
        private NorthwindConnection _connection;

        [SetUp]
        public void SetUp()
        {
            _connection = new NorthwindConnection("Northwind");
        }

        [TearDown]
        public void CleanUp()
        {
            _connection.Dispose();
        }

        [Test]
        public void Add_new_Employee_with_Territories()
        {
            Employee newEmployee = new Employee { FirstName = "Dzianis", LastName = "Shyla" };
            try
            {
                _connection.BeginTransaction();
                newEmployee.EmployeeId = Convert.ToInt32(_connection.InsertWithIdentity(newEmployee));
                _connection.Territories.Where(t => t.TerritoryDescription.Length <= 5)
                    .Insert(_connection.EmployeeTerritories, t => new EmployeeTerritory { EmployeeId = newEmployee.EmployeeId, TerritoryId = t.TerritoryId });
                _connection.CommitTransaction();
            }
            catch
            {
                _connection.RollbackTransaction();
            }
        }

        [Test]
        public void Move_Products_to_another_Category()
        {
            int updatedCount = _connection.Products.Update(p => p.CategoryId == 2, pr => new Product
            {
                CategoryId = 1
            });

            Console.WriteLine(updatedCount);
        }

        [Test]
        public void Insert_list_of_Products_with_Suppliers_and_Categories()
        {
            var products = new List<Product>
            {
                new Product
                {
                    ProductName = "Car",
                    Category = new Category {CategoryName = "Vehicles"},
                    Supplier = new Supplier {CompanyName = "Stark industries"}
                },
                new Product
                {
                    ProductName = "Reactive car",
                    Category = new Category {CategoryName = "Vehicles"},
                    Supplier = new Supplier {CompanyName = "Stark industries"}
                }
            };

            try
            {
                _connection.BeginTransaction();
                //pass ids to products list
                foreach (var product in products)
                {
                    var category = _connection.Categories.FirstOrDefault(c => c.CategoryName == product.Category.CategoryName);
                    product.CategoryId = category?.CategoryId ?? Convert.ToInt32(_connection.InsertWithIdentity(
                                             new Category
                                             {
                                                 CategoryName = product.Category.CategoryName
                                             }));
                    var supplier = _connection.Suppliers.FirstOrDefault(s => s.CompanyName == product.Supplier.CompanyName);
                    product.SupplierId = supplier?.SupplierId ?? Convert.ToInt32(_connection.InsertWithIdentity(
                                             new Supplier
                                             {
                                                 CompanyName = product.Supplier.CompanyName
                                             }));
                }

                _connection.BulkCopy(products);
                _connection.CommitTransaction();
            }
            catch
            {
                _connection.RollbackTransaction();
            }
        }

        [Test]
        public void Replace_Product_with_the_same_in_NotShipped_Orders_plenty_of_queries()
        {
            var orderDetails = _connection.OrderDetails.LoadWith(od => od.Order)
                .Where(od => od.Order.ShippedDate == null).ToList();
            foreach (var orderDetail in orderDetails)
            {
                _connection.OrderDetails.LoadWith(od => od.Product).Update(od => od.OrderId == orderDetail.OrderId && od.ProductId == orderDetail.ProductId,
                    od => new OrderDetail
                    {
                        ProductId = _connection.Products.First(p => !_connection.OrderDetails.Where(t => t.OrderId == od.OrderId)
                                .Any(t => t.ProductId == p.ProductId) && p.CategoryId == od.Product.CategoryId).ProductId
                    });
            }
        }

        [Test]
        public void Replace_Product_with_the_same_in_NotShippedOrders_one_query()
        {
            var updatedRows = _connection.OrderDetails.LoadWith(od => od.Order).LoadWith(od => od.Product)
                .Where(od => od.Order.ShippedDate == null).Update(
                    od => new OrderDetail
                    {
                        ProductId = _connection.Products.First(p => p.CategoryId == od.Product.CategoryId && p.ProductId > od.ProductId) != null
                            ? _connection.Products.First(p => p.CategoryId == od.Product.CategoryId && p.ProductId > od.ProductId).ProductId
                            : _connection.Products.First(p => p.CategoryId == od.Product.CategoryId).ProductId
                    });
            Console.WriteLine($"{updatedRows} rows updated");
        }
    }
}
