using System.Text.Json.Serialization;

namespace ECO.DAL.Entities.Basket
{
    public class BasketItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }

        // Temporary backward compatibility: old cached baskets and old clients still use "qunatity" (the old misspelling). Remove once no old data/client remains.
        [JsonPropertyName("qunatity")]
        public int QuantityLegacy
        {
            get => Quantity;
            set => Quantity = value;
        }

        public string Image {  get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }

    }
}