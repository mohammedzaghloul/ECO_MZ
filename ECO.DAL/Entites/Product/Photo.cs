using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ECO.DAL.Entites.Product
{
    public class Photo:BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

    }
}
