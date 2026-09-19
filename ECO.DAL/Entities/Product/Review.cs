using ECO.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Entities.Product
{
    public class Review : BaseEntity
    {
        public int Rating { get; set; }

        public string? Title { get; set; }

        public string Comment { get; set; } = string.Empty;

        public bool IsVerifiedPurchase { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
