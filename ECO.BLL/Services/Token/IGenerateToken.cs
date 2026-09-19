using ECO.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Token
{
    public interface IGenerateToken
    {
        public Task<string> GenerateTokenAsync(ApplicationUser User, UserManager<ApplicationUser> userManager);

    }
}
