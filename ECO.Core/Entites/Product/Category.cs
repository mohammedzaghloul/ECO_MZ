using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.Core.Entites.Product
{
    public class Category :BaseEntity<int>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Product> Products { get; set; }=new HashSet<Product>();
    }
}
