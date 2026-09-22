using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.DTO.CategoryDtos
{
    public class AddCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
    }
}
