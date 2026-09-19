using ECO.Api.Helper;
using ECO.BLL.DTO.WishlistDtos;
using ECO.BLL.Services.Identity;
using ECO.BLL.Services.WishlistSer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECO.Api.Controller
{
    [Authorize]
    public class WishlistController : BaseController
    {
        private readonly IWishlistService _wishlistService;
        private readonly ICurrentUserService _currentUserService;

        public WishlistController(IWishlistService wishlistService, ICurrentUserService currentUserService)
        {
            _wishlistService = wishlistService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetForUser(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ResponseApi(401, "User is not authenticated."));

            var items = await _wishlistService.GetForUserAsync(userId, cancellationToken);
            return Ok(new GenericResponseApi<IReadOnlyList<WishlistItemDto>>(200, "Wishlist retrieved successfully", items));
        }

        [HttpPost("{productId:int}")]
        public async Task<IActionResult> Add(int productId, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ResponseApi(401, "User is not authenticated."));

            var isAdded = await _wishlistService.AddAsync(userId, productId, cancellationToken);
            if (!isAdded)
                return NotFound(new ResponseApi(404, "Product not found"));

            return Ok(new ResponseApi(200, "Product added to wishlist"));
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ResponseApi(401, "User is not authenticated."));

            var isRemoved = await _wishlistService.RemoveAsync(userId, productId, cancellationToken);
            if (!isRemoved)
                return NotFound(new ResponseApi(404, "Product not found in wishlist"));

            return Ok(new ResponseApi(200, "Product removed from wishlist"));
        }
    }
}
