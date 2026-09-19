using ECO.Api.Helper;
using ECO.BLL.DTO.ReviewDtos;
using ECO.BLL.Services.Identity;
using ECO.BLL.Services.ReviewSer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECO.Api.Controller
{
    public class ReviewsController : BaseController
    {
        private readonly IReviewService _reviewService;
        private readonly ICurrentUserService _currentUserService;

        public ReviewsController(IReviewService reviewService, ICurrentUserService currentUserService)
        {
            _reviewService = reviewService;
            _currentUserService = currentUserService;
        }

        [HttpGet("Product/{productId:int}")]
        public async Task<IActionResult> GetForProduct(int productId, CancellationToken cancellationToken)
        {
            var reviews = await _reviewService.GetForProductAsync(productId, cancellationToken);
            return Ok(new GenericResponseApi<IReadOnlyList<ReviewDto>>(200, "Reviews retrieved successfully", reviews));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddReviewDto dto, CancellationToken cancellationToken)
        {
            if (dto is null)
                return BadRequest(new ResponseApi(400, "Review data is null"));

            try
            {
                var user = await _currentUserService.GetCurrentUser();
                var review = await _reviewService.AddAsync(dto, user, cancellationToken);
                if (review is null)
                    return NotFound(new ResponseApi(404, "Product not found"));

                return Ok(new GenericResponseApi<ReviewDto>(201, "Review submitted successfully", review));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ResponseApi(409, ex.Message));
            }
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDto dto, CancellationToken cancellationToken)
        {
            if (dto is null)
                return BadRequest(new ResponseApi(400, "Review data is null"));

            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ResponseApi(401, "User is not authenticated."));

            var isUpdated = await _reviewService.UpdateAsync(id, userId, dto, cancellationToken);
            if (!isUpdated)
                return NotFound(new ResponseApi(404, "Review not found"));

            return Ok(new ResponseApi(200, "Review updated successfully"));
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ResponseApi(401, "User is not authenticated."));

            var isDeleted = await _reviewService.DeleteAsync(id, userId, cancellationToken);
            if (!isDeleted)
                return NotFound(new ResponseApi(404, "Review not found"));

            return Ok(new ResponseApi(200, "Review deleted successfully"));
        }
    }
}
