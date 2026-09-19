using ECO.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECO.BLL.Services.Token
{
    public class GenerateToken : IGenerateToken
    {
        private readonly IConfiguration configuration;

        public GenerateToken(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<string> GenerateTokenAsync(ApplicationUser User,UserManager<ApplicationUser> userManager)
        {
            var displayName = User.DisplayName ?? User.UserName ?? User.Email ?? User.Id;
            var Authclaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, User.Id),
                new Claim(ClaimTypes.GivenName, displayName),
            };
            if (!string.IsNullOrWhiteSpace(User.Email))
                Authclaims.Add(new Claim(ClaimTypes.Email, User.Email));

            var UserRoles = await userManager.GetRolesAsync(User);
            foreach (var role in UserRoles)
            {
                Authclaims.Add(new Claim(ClaimTypes.Role, role));
            }
            var AuthKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Token:Secret"]));
            var Token = new JwtSecurityToken
                (
                issuer: configuration["Token:Issuer"],
                expires: DateTime.UtcNow.AddMinutes(15),
                claims: Authclaims,
                signingCredentials: new SigningCredentials(AuthKey, SecurityAlgorithms.HmacSha256Signature)
                );
            return new JwtSecurityTokenHandler().WriteToken(Token);
        }
    }
}
