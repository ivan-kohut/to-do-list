using Microsoft.AspNetCore.Identity;

namespace TodoList.Identity.API.Data.Entities
{
  public class UserClaim : IdentityUserClaim<int>
  {
    public User? User { get; set; }
  }
}
