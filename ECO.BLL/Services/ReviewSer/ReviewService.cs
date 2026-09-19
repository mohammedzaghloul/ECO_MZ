using AutoMapper;
using ECO.BLL.DTO.ReviewDtos;
using ECO.BLL.DTO.UserDtos;
using ECO.DAL.Entities.OrderEntities;
using ECO.DAL.Entities.Product;
using ECO.DAL.Interfaces;
using ECO.DAL.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.ReviewSer
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ReviewDto>> GetForProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            var reviews = await _unitOfWork.Repository<Review>()
                .ListAsync(new ReviewSpecification(productId), cancellationToken);
            return _mapper.Map<IReadOnlyList<ReviewDto>>(reviews);
        }

        public async Task<ReviewDto?> AddAsync(AddReviewDto dto, UserDto user, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(dto.ProductId, cancellationToken);
            if (product is null)
                return null;

            var existingReview = await _unitOfWork.Repository<Review>().FirstOrDefaultAsync(
                new ReviewSpecification(dto.ProductId, user.Id), cancellationToken);
            if (existingReview is not null)
                throw new InvalidOperationException("You have already reviewed this product.");

            var hasPurchased = await _unitOfWork.Repository<Order>().FirstOrDefaultAsync(
                OrderSpecification.VerifiedPurchaseForProduct(user.Email!, dto.ProductId), cancellationToken);

            var review = _mapper.Map<Review>(dto);
            review.UserId = user.Id;
            review.IsVerifiedPurchase = hasPurchased is not null;
            review.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Review>().AddAsync(review, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            var reviewDto = _mapper.Map<ReviewDto>(review);
            reviewDto.UserName = user.DisplayName ?? user.Email ?? string.Empty;
            return reviewDto;
        }

        public async Task<bool> UpdateAsync(int reviewId, string userId, UpdateReviewDto dto, CancellationToken cancellationToken = default)
        {
            var review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId, cancellationToken);
            if (review is null || review.UserId != userId)
                return false;

            _mapper.Map(dto, review);

            await _unitOfWork.Repository<Review>().UpdateAsync(review, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(int reviewId, string userId, CancellationToken cancellationToken = default)
        {
            var review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId, cancellationToken);
            if (review is null || review.UserId != userId)
                return false;

            await _unitOfWork.Repository<Review>().DeleteAsync(reviewId, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }
    }
}
