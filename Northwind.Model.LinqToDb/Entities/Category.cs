﻿using System.Collections.Generic;
using LinqToDB.Mapping;

namespace Northwind.Model.LinqToDb.Entities
{
    [Table("[dbo].[Categories]")]
    public class Category
    {
        [PrimaryKey]
        [Identity]
        [Column("CategoryID")]
        public int CategoryId { get; set; }

        [Column("CategoryName")]
        public string CategoryName { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("Picture")]
        public byte[] Picture { get; set; }

        [Association(ThisKey = nameof(CategoryId), OtherKey = nameof(Entities.Product.SupplierId), CanBeNull = true)]
        public IEnumerable<Product> Products { get; set; }
    }

}