using ECO.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Entities
{
    public class Discount : BaseEntity
    {
        public string Code { get; set; } = string.Empty;

        // True: Value is a percentage (1-100). False: Value is a fixed amount.
        public bool IsPercentage { get; set; }

        public decimal Value { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? ExpiryDate { get; set; }

        public int? MaxUses { get; set; }

        public int UsedCount { get; set; }
    }
}
