using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Entities.Basket
{
    public class CustomerBasket
    {
        public CustomerBasket()
        {

        }
        public CustomerBasket(string id)
        {
            this.Id=id;
        }
        public string PaymentIntentId { get; set; }
        public string ClientSecret { get; set; }
        public string Id { get; set; }
        public string? CouponCode { get; set; }
        public List<BasketItem> BasketItems {  get; set; }=new List<BasketItem>();  
    }
}
