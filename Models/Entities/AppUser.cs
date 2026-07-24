using Microsoft.AspNetCore.Identity;

namespace IpGroups.Models.Entities
{
    public class AppUser : IdentityUser
    {
        public string AdSoyad { get; set; } = string.Empty;
        public string? Gender { get; set; } // "Kadın" or "Erkek"
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
