using ECO.BLL.DTO.OrderDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.DTO.AdminDtos
{
    public class CustomerListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class CustomerDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public int OrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
        public List<OrderToReturnDto> Orders { get; set; } = new();
    }

    public class UpdateAdminRoleDto
    {
        public bool AddToRole { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
