using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Entities.Product
{
    public class Category :BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new HashSet<Category>();
        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
}
