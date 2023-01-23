using Microsoft.AspNetCore.Identity;

namespace TodoList.Identity.API.Data.Entities
{
    public class UserLogin : IdentityUserLogin<int>
    {
        public User? User { get; set; }
    }
}
