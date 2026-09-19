using ECO.BLL.DTO.WishlistDtos;
using ECO.DAL.Entities.Product;
using ECO.DAL.Interfaces;
using ECO.DAL.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECO.BLL.Services.WishlistSer
{
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WishlistService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<WishlistItemDto>> GetForUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            var wishlistItems = await _unitOfWork.Repository<WishlistItem>()
                .ListAsync(new WishlistItemSpecification(userId), cancellationToken);

            if (wishlistItems.Count == 0)
                return Array.Empty<WishlistItemDto>();

            var productIds = wishlistItems.Select(item => item.ProductId).Distinct().ToList();
            var products = await _unitOfWork.ProductRepository.GetByIdsAsync(productIds, cancellationToken);
            var productsById = products.ToDictionary(product => product.Id);

            return wishlistItems
                .Select(item =>
                {
                    productsById.TryGetValue(item.ProductId, out var product);
                    return new WishlistItemDto
                    {
                        ProductId = item.ProductId,
                        Name = product?.Name ?? item.Product.Name,
                        NewPrice = product?.NewPrice ?? item.Product.NewPrice,
                        OldPrice = product?.OldPrice ?? item.Product.OldPrice,
                        Photo = (product?.Photos ?? item.Product.Photos)?
                            .OrderBy(photo => photo.Id)
                            .FirstOrDefault()?.Name,
                        AddedAt = item.CreatedAt
                    };
                })
                .ToList();
        }

        public async Task<bool> AddAsync(string userId, int productId, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId, cancellationToken);
            if (product is null)
                return false;

            var existingItem = await _unitOfWork.Repository<WishlistItem>().FirstOrDefaultAsync(
                new WishlistItemSpecification(userId, productId), cancellationToken);
            if (existingItem is not null)
                return true;

            var wishlistItem = new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<WishlistItem>().AddAsync(wishlistItem, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RemoveAsync(string userId, int productId, CancellationToken cancellationToken = default)
        {
            var wishlistItem = await _unitOfWork.Repository<WishlistItem>().FirstOrDefaultAsync(
                new WishlistItemSpecification(userId, productId), cancellationToken);
            if (wishlistItem is null)
                return false;

            await _unitOfWork.Repository<WishlistItem>().DeleteAsync(wishlistItem.Id, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }
    }
}
