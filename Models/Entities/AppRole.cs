using Microsoft.AspNetCore.Identity;

namespace IpGroups.Models.Entities
{
    public class AppRole : IdentityRole
    {
        public string? Description { get; set; }
    }
}
