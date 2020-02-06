using API.Models;

namespace TodoList.Client
{
  public class ApiUrls
  {
    public static readonly string GetUsersList = Urls.Users;
    public static readonly string DeleteUser = $"{Urls.Users}/{Urls.DeleteUser}";
    public static readonly string Login = $"{Urls.Users}/{Urls.Login}";
    public static readonly string SignUp = Urls.Users;
    public static readonly string PasswordRecovery = $"{Urls.Users}/{Urls.PasswordRecovery}";
    public static readonly string LoginByFacebook = $"{Urls.Users}/{Urls.LoginByFacebook}";
    public static readonly string LoginByGoogle = $"{Urls.Users}/{Urls.LoginByGoogle}";
    public static readonly string LoginByGithub = $"{Urls.Users}/{Urls.LoginByGithub}";
    public static readonly string LoginByLinkedin = $"{Urls.Users}/{Urls.LoginByLinkedin}";
    public static readonly string ChangePassword = $"{Urls.Users}/{Urls.ChangePassword}";
    public static readonly string IsTwoFactorAuthenticationEnabled = $"{Urls.Users}/{Urls.IsTwoFactorAuthenticationEnabled}";
    public static readonly string AuthenticatorUri = $"{Urls.Users}/{Urls.AuthenticatorUri}";
    public static readonly string EnableTwoFactorAuthentication = $"{Urls.Users}/{Urls.EnableTwoFactorAuthentication}";
    public static readonly string DisableTwoFactorAuthentication = $"{Urls.Users}/{Urls.DisableTwoFactorAuthentication}";

    public static readonly string GetItemsList = Urls.Items;
    public static readonly string CreateItem = Urls.Items;
    public static readonly string UpdateItem = $"{Urls.Items}/{Urls.UpdateItem}";
    public static readonly string DeleteItem = $"{Urls.Items}/{Urls.DeleteItem}";
  }
}
