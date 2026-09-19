namespace ECO.DAL.Entities.Product
{
    public class ProductSpecItem : BaseEntity
    {
        public int ProductId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public virtual Product Product { get; set; } = null!;
    }
}
