using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Sharing
{
    public class ProductParams
    {
        public string? Sort { get; set; }
        public int? CategoryId { get; set; }
        public int? PageNumber { get; set; } = 1;
        private int? _PageSize;

        public int? PageSize
        {
            get { return _PageSize; }
            set { _PageSize = value>MaxPageSize ? MaxPageSize:value; }
        }


        public int MaxPageSize { get; set; } = 100;

        public string? Search { get; set; }

        public string? Serach
        {
            get => Search;
            set => Search = value;
        }
    }
}
