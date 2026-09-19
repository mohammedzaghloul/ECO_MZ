using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.DTO.Auth
{
    public class ResetPasswordDto : LoginDto
    {
        public string Token { get; set; }
    }
}
