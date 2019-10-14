using Microsoft.AspNetCore.Identity;

namespace Entities
{
  public class RoleClaim : IdentityRoleClaim<int>
  {
    public Role Role { get; set; } = null!;
  }
}
