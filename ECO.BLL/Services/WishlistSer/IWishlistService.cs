using ECO.BLL.DTO.WishlistDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.WishlistSer
{
    public interface IWishlistService
    {
        Task<IReadOnlyList<WishlistItemDto>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(string userId, int productId, CancellationToken cancellationToken = default);
        Task<bool> RemoveAsync(string userId, int productId, CancellationToken cancellationToken = default);
    }
}
