using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Entities
{
  public class User : IdentityUser<int>
  {
    public ICollection<Item> Items { get; set; }
  }
}
