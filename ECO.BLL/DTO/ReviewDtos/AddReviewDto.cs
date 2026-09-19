using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECO.BLL.DTO.ReviewDtos
{
    public class AddReviewDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(150)]
        public string? Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }
}
