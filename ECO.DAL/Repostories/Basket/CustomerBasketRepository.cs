using ECO.DAL.Data;
using ECO.DAL.Entites.Basket;
using ECO.DAL.Interfaces.Basket;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ECO.DAL.Repostories.Basket
{
    public class CustomerBasketRepository : ICustomerBasketRepository
    {
        private readonly StackExchange.Redis.IDatabase _database;
        public CustomerBasketRepository(IConnectionMultiplexer redis )
        {
            _database = redis.GetDatabase();
        }
        public async Task<CustomerBasket> GetBasketAsync(string id)
        {
             var result = await _database.StringGetAsync(id);
            if (!string.IsNullOrEmpty(result))
                return JsonSerializer.Deserialize<CustomerBasket>((byte[])result);
            return null;
        }

        public  async Task<bool> DeleteAsync(string id)
        {
            return await  _database.KeyDeleteAsync(id);    
        }

        public Task<CustomerBasket> UpdateAsync(CustomerBasket customerBasket)
        {
           var basketValue= JsonSerializer.Serialize(customerBasket);
            var basket = _database.StringSetAsync(customerBasket.Id, basketValue,TimeSpan.FromDays(7));
            if (basket != null)
                return GetBasketAsync(customerBasket.Id);
            return null;
        }
    }
}
