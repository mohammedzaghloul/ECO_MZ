using ECO.BLL.DTO.ReviewDtos;
using ECO.BLL.DTO.UserDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.ReviewSer
{
    public interface IReviewService
    {
        Task<IReadOnlyList<ReviewDto>> GetForProductAsync(int productId, CancellationToken cancellationToken = default);
        Task<ReviewDto?> AddAsync(AddReviewDto dto, UserDto user, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(int reviewId, string userId, UpdateReviewDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int reviewId, string userId, CancellationToken cancellationToken = default);
    }
}
