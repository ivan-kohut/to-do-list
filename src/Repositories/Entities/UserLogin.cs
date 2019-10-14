using Microsoft.AspNetCore.Identity;

namespace Entities
{
  public class UserLogin : IdentityUserLogin<int>
  {
    public User User { get; set; } = null!;
  }
}
