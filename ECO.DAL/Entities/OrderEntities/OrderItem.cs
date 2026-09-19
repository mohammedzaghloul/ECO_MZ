namespace ECO.DAL.Entities.OrderEntities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public string ProductItemId { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public string ProductName { get; set; }

        public string MainImage { get; set; }

        public OrderItem()
        {
        }

        public OrderItem(int id, decimal price, int quantity, string productName, string mainImage)
        {
            ProductItemId = id.ToString();
            Price = price;
            Quantity = quantity;
            ProductName = productName;
            MainImage = mainImage;
        }
    }
}