using Microsoft.AspNetCore.Identity;

namespace TodoList.Identity.API.Data.Entities
{
    public class RoleClaim : IdentityRoleClaim<int>
    {
        public Role? Role { get; set; }
    }
}
