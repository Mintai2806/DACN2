using Microsoft.AspNetCore.Identity;

namespace Shopping_Tutorial.Models
{
    public class UserWithRoleName
    {
        public IdentityUser User { get; set; }
        public string RoleName { get; set; }

    }
}
