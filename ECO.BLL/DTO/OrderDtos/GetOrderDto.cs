using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.DTO.OrderDtos
{
    public class GetOrderDto
    {
        public string? status { get; set; }
        public string? search { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
        public int pageNumber { get; set; } = 10;
        public int pageSize { get; set; } = 1;
    }
}
