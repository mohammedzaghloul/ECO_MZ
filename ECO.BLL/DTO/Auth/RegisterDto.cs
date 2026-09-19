using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.DTO.Auth
{
    public class RegisterDto :LoginDto
    {
        public string UserName { get; set; }
    }
}
