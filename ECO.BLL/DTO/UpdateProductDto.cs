using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ECO.BLL.DTO
{
    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public int CategoryId { get; set; }
        public IFormFileCollection? Photos { get; set; } = null;
    }
}
