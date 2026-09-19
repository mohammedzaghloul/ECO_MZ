using ECO.BLL.DTO.Auth;
using ECO.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Identity
{
    public interface IAuthService
    {
       Task<string>  RegisterAsync(RegisterDto register);
       Task<string> LoginAsync(LoginDto register);
       Task SendEmail(string email, string code, string component, string subject, string Message);
       Task<bool> SendEmailAndForgetPassword(string email);
       Task<string> ResetPassword(ResetPasswordDto resetPassword);
       Task<bool> ActiveAccount(ActiveAccountDto activeAccountDto);
    }
}
