namespace ECO.BLL.DTO.ProductDtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool TrackStock { get; set; }
        public int? StockQuantity { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public List<string> Photos { get; set; } = new List<string>();
        public string Description { get; set; } 
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<ProductSpecificationDto> Specifications { get; set; } = new();
    }

    public class ProductSpecificationDto
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
