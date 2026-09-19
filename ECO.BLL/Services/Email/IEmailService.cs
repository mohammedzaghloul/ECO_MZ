using ECO.BLL.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Email
{
    public interface IEmailService
    {
        Task SendEmail(EmailDto emailDto);
    }
}
