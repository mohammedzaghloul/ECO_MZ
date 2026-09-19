
using ECO.BLL.DTO.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.DTO.OrderDtos
{
    public class OrderDto
    {
        public int DeliveryMethodId { get; set; }
        public int GovernorateId { get; set; }
        public int CityId { get; set; }
        public string BasketId { get; set; }
        public ShippingAddressDto ShippingAddressDto { get; set; }
        // Stripe (default) | InstaPay | VodafoneCash | COD
        public string? PaymentMethod { get; set; }
    }
}
