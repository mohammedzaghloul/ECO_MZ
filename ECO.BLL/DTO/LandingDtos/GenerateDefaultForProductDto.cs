using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.DTO.LandingDtos
{
    public class GenerateDefaultForProductDto
    {
      public  int productId { set; get; }
      public string productName { set; get; }
      public  string? description { set; get; }
      public decimal price { set; get; }
      public   string? mainImageUrl { set; get; }
    }
}
