using ECO.DAL.Entities.Basket;
using ECO.BLL.DTO.Basket;
using ECO.DAL.Interfaces;
using ECO.DAL.Repositories;
using ECO.BLL.Constants;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Basket
{
    public class BasketService : IBasketService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly BasketSettings settings;

        public BasketService(IUnitOfWork unitOfWork, IOptions<BasketSettings> options)
        {
            this.unitOfWork = unitOfWork;
            settings = options.Value;
        }

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
           return unitOfWork.CustomerBasketRepository.DeleteAsync(id, cancellationToken);
        }

        public Task<CustomerBasket> GetBasketAsync(string id, CancellationToken cancellationToken = default)
        {
            return unitOfWork.CustomerBasketRepository.GetBasketAsync(id, cancellationToken);
        }

        public async Task<CustomerBasket> UpdateAsync(
            UpdateBasketDto basketDto,
            CancellationToken cancellationToken = default)
        {
            if (basketDto == null)
                throw new ArgumentNullException(nameof(basketDto));
            if (string.IsNullOrWhiteSpace(basketDto.Id))
                throw new ArgumentException("Basket id is required", nameof(basketDto));

            var items = basketDto.Items ?? [];
            if (items.Any(item =>
                item.ProductId <= 0 ||
                item.Quantity <= 0 ||
                item.Quantity > settings.MaxQuantityPerItem))
            {
                throw new ArgumentException(
                    OrderErrorMessages.InvalidQuantity(1, settings.MaxQuantityPerItem));
            }

            var productIds = items.Select(item => item.ProductId).Distinct().ToList();
            var products = await unitOfWork.ProductRepository
                .GetByIdsAsync(productIds, cancellationToken);
            var productsById = products.ToDictionary(product => product.Id);

            if (productsById.Count != productIds.Count)
            {
                var missingProductIds = productIds
                    .Except(productsById.Keys)
                    .ToList();
                throw new InvalidOperationException(
                    OrderErrorMessages.ProductsUnavailableFor(missingProductIds));
            }

            var basket = new CustomerBasket(basketDto.Id);

            // Preserve payment intent + coupon across item updates.
            var existingBasket = await unitOfWork.CustomerBasketRepository
                .GetBasketAsync(basketDto.Id, cancellationToken);
            basket.PaymentIntentId = existingBasket?.PaymentIntentId ?? string.Empty;
            basket.ClientSecret = existingBasket?.ClientSecret ?? string.Empty;
            basket.CouponCode = basketDto.CouponCode ?? existingBasket?.CouponCode;

            var insufficientStockProductIds = settings.EnableStockValidation
                ? items
                    .Where(item =>
                        productsById[item.ProductId].TrackStock &&
                        (productsById[item.ProductId].StockQuantity ?? 0) < item.Quantity)
                    .Select(item => item.ProductId)
                    .ToList()
                : [];

            if (insufficientStockProductIds.Count > 0)
                throw new InvalidOperationException(
                    OrderErrorMessages.InsufficientStockFor(insufficientStockProductIds));

            foreach (var item in items)
            {
                var product = productsById[item.ProductId];
                basket.BasketItems.Add(new BasketItem
                {
                    Id = product.Id,
                    Name = product.Name,
                    Quantity = item.Quantity,
                    Price = product.NewPrice,
                    Image = product.Photos.FirstOrDefault()?.Name ?? string.Empty,
                    Category = product.Category?.Name ?? product.CategoryId.ToString()
                });
            }

            return await unitOfWork.CustomerBasketRepository.UpdateAsync(
                basket,
                cancellationToken);
        }
    }
}
