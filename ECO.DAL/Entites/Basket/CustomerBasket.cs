using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Entites.Basket
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
        public string Id { get; set; }
        public List<BasketItem> BasketItems {  get; set; }=new List<BasketItem>();  
    }
}
