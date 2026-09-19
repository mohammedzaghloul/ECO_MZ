using ECO.BLL.DTO.UserDtos;
using ECO.BLL.DTO.UserDtos;
using ECO.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ECO.BLL.Services.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public CurrentUserService(IHttpContextAccessor  httpContextAccessor,UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        public string? UserId =>
         _httpContextAccessor.HttpContext?
             .User.FindFirstValue(ClaimTypes.NameIdentifier);

        public string? Email =>
            _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.Email);

        public async Task<UserDto> GetCurrentUser()
        {
            var userId = UserId;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not authenticated.");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            var displayName = !string.IsNullOrWhiteSpace(user.DisplayName)
                ? user.DisplayName
                : _httpContextAccessor.HttpContext?
                    .User.FindFirstValue(ClaimTypes.GivenName)
                    ?? user.UserName;

            return new UserDto
            {
                DisplayName = displayName,
                Email = user.Email,
                Id=user.Id,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            };
        }
    }
}
