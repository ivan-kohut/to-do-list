using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace TodoList.Identity.API.Data.Entities
{
    public class User : IdentityUser<int>
    {
        public ICollection<UserRole>? UserRoles { get; set; }

        public ICollection<UserLogin>? UserLogins { get; set; }

        public ICollection<UserToken>? UserTokens { get; set; }

        public ICollection<UserClaim>? UserClaims { get; set; }
    }
}
