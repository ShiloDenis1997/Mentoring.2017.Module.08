using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Northwind.Model.EF;
using NUnit.Framework;

namespace Task2.Demonstration
{
    [TestFixture]
    public class Task1
    {
        private NorthwindContext db;

        [SetUp]
        public void SetUp()
        {
            db = new NorthwindContext();
        }

        [TearDown]
        public void CleanUp()
        {
            db.Dispose();
        }

        [Test]
        public void Orders_with_products_of_concrete_category()
        {
            int selectedCategoryId = 3;
            var query = db.Orders.Include(o => o.Order_Details.Select(od => od.Product)).Include(o => o.Customer)
                .Where(o => o.Order_Details.Any(od => od.Product.CategoryID == selectedCategoryId))
                .Select(o => new
                {
                    o.Customer.ContactName,
                    Order_Details = o.Order_Details.Select(od => new
                    {
                        od.Product.ProductName,
                        od.OrderID,
                        od.Discount,
                        od.Quantity,
                        od.UnitPrice,
                        od.ProductID
                    })
                });
            var result = query.ToList();

            foreach (var row in result)
            {
                Console.WriteLine($"Customer: {row.ContactName} Products: {string.Join(", ", row.Order_Details.Select(od => od.ProductName))}");
            }
        }
    }
}
