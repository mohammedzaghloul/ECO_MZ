using ECO.BLL.DTO.Order;
using ECO.DAL.Entities.OrderEntities;

namespace ECO.BLL.DTO.OrderDtos
{
    public class OrderToReturnDto
    {
        public int Id { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerPhone { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingPrice { get; set; }
        public int DeliveryMethodId { get; set; }
        public string DeliveryMethodName { get; set; }
        public ShippingAddressDto ShippingAddress { get; set; }
        public IReadOnlyList<OrderItemDto> OrderItems { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string MainImage { get; set; }
    }
}
