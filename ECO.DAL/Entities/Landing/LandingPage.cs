using ECO.DAL.Entities.Product;
using ProductEntity = ECO.DAL.Entities.Product.Product;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Entities.Landing
{
    public class LandingPage : BaseEntity
    {
        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public int? ProductId { get; set; }
        public virtual ProductEntity? Product { get; set; }
        public string Template { get; set; } = "minimal";
        public string AccentColor { get; set; } = "#4f46e5";
        public string FontFamily { get; set; } = "system";
        public string VideoUrl { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
        public string WhatsAppMessage { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        public int ViewCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual List<LandingPageSection> Sections { get; set; } = new();
    }
}
