using Microsoft.AspNetCore.Identity;

namespace ECO.DAL.Entities
{
    public class ApplicationUser :IdentityUser
    {
        public string? DisplayName { get; set; }
        public virtual Address Address { get; set; }
    }
}
