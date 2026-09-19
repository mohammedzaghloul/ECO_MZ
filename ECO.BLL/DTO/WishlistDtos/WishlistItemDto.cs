using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.DTO.WishlistDtos
{
    public class WishlistItemDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public string? Photo { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
