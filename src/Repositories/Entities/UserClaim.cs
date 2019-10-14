using Microsoft.AspNetCore.Identity;

namespace Entities
{
  public class UserClaim : IdentityUserClaim<int>
  {
    public User User { get; set; } = null!;
  }
}
