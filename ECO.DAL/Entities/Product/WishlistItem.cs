using ECO.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Entities.Product
{
    public class WishlistItem : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
