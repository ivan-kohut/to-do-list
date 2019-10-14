using System.Collections.Generic;

namespace Services
{
  public class UserDTO
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsRegisteredInSystem { get; set; }
    public bool IsEmailConfirmed { get; set; }

    public IEnumerable<string> LoginProviders { get; set; } = null!;
  }
}
