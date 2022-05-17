using System.Collections.Generic;

namespace TodoList.Identity.API.ViewModels
{
  public class UsersViewModel
  {
    public IEnumerable<UserViewModel>? Users { get; set; }
  }

  public class UserViewModel
  {
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsRegisteredInSystem { get; set; }

    public bool IsLoggedInViaFacebook { get; set; }

    public bool IsLoggedInViaGoogle { get; set; }

    public bool IsLoggedInViaGithub { get; set; }

    public bool IsEmailConfirmed { get; set; }
  }
}
