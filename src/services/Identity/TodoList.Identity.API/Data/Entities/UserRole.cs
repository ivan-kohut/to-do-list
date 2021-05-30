using Microsoft.AspNetCore.Identity;

namespace TodoList.Identity.API.Data.Entities
{
  public class UserRole : IdentityUserRole<int>
  {
    public User? User { get; set; }
    public Role? Role { get; set; }
  }
}
