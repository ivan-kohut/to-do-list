namespace API.Models
{
  public class UserListApiModel
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsRegisteredInSystem { get; set; }
    public bool IsLoggedInViaFacebook { get; set; }
    public bool IsLoggedInViaGoogle { get; set; }
    public bool IsLoggedInViaGithub { get; set; }
    public bool IsLoggedInViaLinkedin { get; set; }
    public bool IsEmailConfirmed { get; set; }
  }
}
