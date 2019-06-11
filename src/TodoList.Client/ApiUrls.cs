using API.Models;

namespace TodoList.Client
{
  public class ApiUrls
  {
    private const string apiUrl = "https://localhost:44388";

    public static readonly string Login = $"{apiUrl}{Urls.Users}/{Urls.Login}";
    public static readonly string SignUp = $"{apiUrl}{Urls.Users}";
    public static readonly string PasswordRecovery = $"{apiUrl}{Urls.Users}/{Urls.PasswordRecovery}";
    public static readonly string LoginByFacebook = $"{apiUrl}{Urls.Users}/{Urls.LoginByFacebook}";
    public static readonly string LoginByGoogle = $"{apiUrl}{Urls.Users}/{Urls.LoginByGoogle}";
    public static readonly string LoginByGithub = $"{apiUrl}{Urls.Users}/{Urls.LoginByGithub}";
    public static readonly string LoginByLinkedin = $"{apiUrl}{Urls.Users}/{Urls.LoginByLinkedin}";
  }
}
