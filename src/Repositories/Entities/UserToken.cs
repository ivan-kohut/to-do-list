using Microsoft.AspNetCore.Identity;

namespace Entities
{
  public class UserToken : IdentityUserToken<int>
  {
    public User User { get; set; } = null!;
  }
}
