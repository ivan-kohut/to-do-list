using System.Collections.Generic;

namespace Entities
{
  public class User
  {
    public int Id { get; set; }

    public ICollection<Item> Items { get; set; } = null!;
  }
}
