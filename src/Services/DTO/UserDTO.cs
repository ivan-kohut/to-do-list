using System.Collections.Generic;

namespace Services
{
  public class UserDTO
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsRegisteredInSystem { get; set; }
    public bool IsEmailConfirmed { get; set; }

    public IEnumerable<string> LoginProviders { get; set; }
  }
}
