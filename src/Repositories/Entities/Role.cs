using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Entities
{
  public class Role : IdentityRole<int>
  {
    public ICollection<UserRole> UserRoles { get; set; } = null!;
    public ICollection<RoleClaim> RoleClaims { get; set; } = null!;
  }
}
