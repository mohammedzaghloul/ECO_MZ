using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.DTO.Basket
{
    public class ApplyCouponDto
    {
        public string BasketId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class CouponResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Code { get; set; }
        public bool IsPercentage { get; set; }
        public decimal Value { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
