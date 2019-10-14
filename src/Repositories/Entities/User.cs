using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Entities
{
  public class User : IdentityUser<int>
  {
    public ICollection<UserRole> UserRoles { get; set; } = null!;
    public ICollection<UserLogin> UserLogins { get; set; } = null!;
    public ICollection<UserToken> UserTokens { get; set; } = null!;
    public ICollection<UserClaim> UserClaims { get; set; } = null!;
    public ICollection<Item> Items { get; set; } = null!;
  }
}
