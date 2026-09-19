using ECO.DAL.Entities.Product;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ECO.DAL.Specifications
{
    public class WishlistItemSpecification : BaseSpecification<WishlistItem>
    {
        public WishlistItemSpecification(string userId)
            : base(wishlistItem => wishlistItem.UserId == userId)
        {
            AddInclude(wishlistItem => wishlistItem.Product);
            ApplyOrderByDescending(wishlistItem => wishlistItem.CreatedAt);
        }

        public WishlistItemSpecification(string userId, int productId)
            : base(wishlistItem => wishlistItem.UserId == userId && wishlistItem.ProductId == productId)
        {
        }
    }
}
