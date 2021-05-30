using Microsoft.AspNetCore.Identity;

namespace TodoList.Identity.API.Data.Entities
{
  public class UserToken : IdentityUserToken<int>
  {
    public User? User { get; set; }
  }
}
