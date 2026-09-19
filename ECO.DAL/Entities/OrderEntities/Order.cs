using Stripe;

namespace ECO.DAL.Entities.OrderEntities
{
    public class Order :BaseEntity
    {
        public Order()
        {
            
        }
        public Order(string buyerEmail, string basketId, decimal subTotal, ShippingAddress shippingAddress, DeliveryMethod deliveryMethod, IReadOnlyList<OrderItem> orderItems,string PaymentIntentId 
            )
        {
            BuyerEmail = buyerEmail;
            BasketId = basketId;
            SubTotal = subTotal;
            ShippingAddress = shippingAddress;
            DeliveryMethod = deliveryMethod;
            OrderItems = orderItems;
            this.PaymentIntentId = PaymentIntentId;
        }
       public string PaymentIntentId { get; set; }
        // Stripe | InstaPay | VodafoneCash | COD
        public string PaymentMethod { get; set; } = "Stripe";
        public string BuyerEmail { get; set; }
        public string? BuyerPhone { get; set; }
        public int? LandingPageId { get; set; }
        public virtual ECO.DAL.Entities.Landing.LandingPage? LandingPage { get; set; }
        public string? BasketId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingPrice { get; set; }
        public decimal Discount { get; set; }
        public DateTime OrderDate { get; set; }= DateTime.Now;
        public ShippingAddress  ShippingAddress { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public IReadOnlyList<OrderItem> OrderItems {  get; set; }
        public Status Status { get; set; } = Status.Pending;
        public decimal GetTotal()
        {
            var total = SubTotal + ShippingPrice - Discount;
            return total < 0 ? 0 : total;
        }
    }
}
