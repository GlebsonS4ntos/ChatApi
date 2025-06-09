using Microsoft.AspNetCore.Identity;

namespace ChatApi.Models
{
    public class User : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime? ValidRefreshToken { get; set; }
    }
}