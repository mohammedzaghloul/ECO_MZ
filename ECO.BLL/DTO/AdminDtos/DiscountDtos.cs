using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECO.BLL.DTO.AdminDtos
{
    public class DiscountDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public bool IsPercentage { get; set; }
        public decimal Value { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? MaxUses { get; set; }
        public int UsedCount { get; set; }
    }

    public class SaveDiscountDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string Code { get; set; } = string.Empty;

        public bool IsPercentage { get; set; }

        [Range(0.01, 10000)]
        public decimal Value { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? ExpiryDate { get; set; }

        [Range(1, 1000)]
        public int? MaxUses { get; set; }
    }
}
