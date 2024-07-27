using Microsoft.AspNetCore.Identity;

namespace FileSharing.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
