using ECO.DAL.Entities.Product;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ECO.DAL.Specifications
{
    public class ReviewSpecification : BaseSpecification<Review>
    {
        public ReviewSpecification(int productId)
            : base(review => review.ProductId == productId)
        {
            AddInclude(review => review.User);
            ApplyOrderByDescending(review => review.CreatedAt);
        }

        public ReviewSpecification(int productId, string userId)
            : base(review => review.ProductId == productId && review.UserId == userId)
        {
        }
    }
}
