using ECO.DAL.Data;
using ECO.DAL.Entities.Basket;
using ECO.DAL.Interfaces.Basket;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ECO.DAL.Repositories.Basket
{
    public class CustomerBasketRepository : ICustomerBasketRepository
    {
        // Case-insensitive matching so baskets cached under the old misspelled property names still deserialize.
        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly StackExchange.Redis.IDatabase _database;
        public CustomerBasketRepository(IConnectionMultiplexer redis )
        {
            _database = redis.GetDatabase();
        }
        public async Task<CustomerBasket> GetBasketAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
             cancellationToken.ThrowIfCancellationRequested();
             var result = await _database.StringGetAsync(id);
             cancellationToken.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(result))
                return JsonSerializer.Deserialize<CustomerBasket>((byte[])result, jsonOptions);
            return null;
        }

        public async Task<bool> DeleteAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var deleted = await _database.KeyDeleteAsync(id);
            cancellationToken.ThrowIfCancellationRequested();
            return deleted;
        }

        public async Task<CustomerBasket> UpdateAsync(
            CustomerBasket customerBasket,
            CancellationToken cancellationToken = default)
        {
           cancellationToken.ThrowIfCancellationRequested();
           var basketValue= JsonSerializer.Serialize(customerBasket, jsonOptions);
           var saved = await _database.StringSetAsync(
              customerBasket.Id,
              basketValue,
              TimeSpan.FromDays(7));
           cancellationToken.ThrowIfCancellationRequested();
           return saved ? await GetBasketAsync(customerBasket.Id, cancellationToken) : null;
        }
    }
}
