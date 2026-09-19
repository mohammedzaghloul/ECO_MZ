using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ECO.BLL.DTO.ProductDtos
{
    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public int CategoryId { get; set; }
        public bool TrackStock { get; set; }
        public int? StockQuantity { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public IFormFileCollection? Photos { get; set; } = null;
        public string Specifications { get; set; } = "[]";
    }
}
