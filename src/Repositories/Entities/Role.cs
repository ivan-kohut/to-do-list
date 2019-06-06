﻿using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Entities
{
  public class Role : IdentityRole<int>
  {
    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<RoleClaim> RoleClaims { get; set; }
  }
}