using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECO.BLL.DTO.LandingDtos
{
    public class LandingSectionDto
    {
        public int? Id { get; set; }

        [Required]
        public string SectionType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsVisible { get; set; } = true;

        public string ImageUrl { get; set; } = string.Empty;

        // JSON payload: { headline, body, buttonText, buttonLink, items: [...] }
        public string ContentJson { get; set; } = "{}";
    }

    public class LandingPageDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? ProductId { get; set; }
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
        public List<LandingSectionDto> Sections { get; set; } = new();
    }

    public class SaveLandingPageDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(180)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(180)]
        public string Slug { get; set; } = string.Empty;

        public int? ProductId { get; set; }
        public string Template { get; set; } = "minimal";
        public string AccentColor { get; set; } = "#4f46e5";
        public string FontFamily { get; set; } = "system";
        public string VideoUrl { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
        public string WhatsAppMessage { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public List<LandingSectionDto> Sections { get; set; } = new();
    }

    /// <summary>Lightweight row for the admin list view.</summary>
    public class LandingPageSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
        public int OrdersCount { get; set; }
        public decimal ConversionRate { get; set; }
        public int SectionsCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
