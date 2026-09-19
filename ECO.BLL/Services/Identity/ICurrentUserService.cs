using ECO.BLL.DTO.UserDtos;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Identity
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Email { get; }
        public  Task<UserDto> GetCurrentUser();

    }
}
