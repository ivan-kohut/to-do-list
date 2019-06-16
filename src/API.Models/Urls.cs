namespace API.Models
{
  public class Urls
  {
    public const string Users = "/api/v1/users";

    public const string Login = "login";
    public const string PasswordRecovery = "password";
    public const string LoginByFacebook = "login-by-facebook";
    public const string LoginByGoogle = "login-by-google";
    public const string LoginByGithub = "login-by-github";
    public const string LoginByLinkedin = "login-by-linkedin";
    public const string ChangePassword = "password";
    public const string IsTwoFactorAuthenticationEnabled = "two-factor-authentication/is-enabled";
    public const string AuthenticatorUri = "authenticator-uri";
    public const string EnableTwoFactorAuthentication = "two-factor-authentication/enable";
    public const string DisableTwoFactorAuthentication = "two-factor-authentication/disable";
    public const string DeleteUser = "{id}";

    public const string Items = "/api/v1/items";

    public const string UpdateItem = "{id}";
    public const string DeleteItem = "{id}";
  }
}
