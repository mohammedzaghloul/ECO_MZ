using ECO.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ECO.DAL.Specifications
{
    public class DiscountSpecification : BaseSpecification<Discount>
    {
        public DiscountSpecification(string code)
            : base(discount => discount.Code == code)
        {
        }
    }
}
