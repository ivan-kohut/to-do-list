namespace TodoList.Client.Components
{
  public class FacebookCallbackComponent : CallbackComponentBase
  {
    protected override string ApiUri => ApiUrls.LoginByFacebook;

    protected override string RelativeRedirectUri => "/facebook-callback";
  }
}
