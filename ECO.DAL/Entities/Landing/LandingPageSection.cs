using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Entities.Landing
{
    public class LandingPageSection : BaseEntity
    {
        public int LandingPageId { get; set; }
        public virtual LandingPage LandingPage { get; set; } = null!;

        // hero, trustbar, features, showcase, reviews, faq, cta, orderform
        public string SectionType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsVisible { get; set; } = true;

        public string ImageUrl { get; set; } = string.Empty;

        // Flexible per-type content:
        // { "headline": "...", "body": "...", "buttonText": "...", "buttonLink": "...", "items": [ ... ] }
        public string ContentJson { get; set; } = "{}";
    }
}
