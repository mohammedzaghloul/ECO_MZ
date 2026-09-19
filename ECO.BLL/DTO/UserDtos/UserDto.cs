using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.DTO.UserDtos
{
    public class UserDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
