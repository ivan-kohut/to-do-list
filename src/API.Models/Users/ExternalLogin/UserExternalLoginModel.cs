namespace API.Models
{
  public class UserExternalLoginModel
  {
    public string Code { get; set; } = null!;
    public string? RedirectUri { get; set; }
  }
}
