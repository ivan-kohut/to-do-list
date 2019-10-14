namespace TodoList.Client.Components
{
  public class LinkedinCallbackComponent : CallbackComponentBase
  {
    protected override string ApiUri => ApiUrls.LoginByLinkedin;

    protected override string? RelativeRedirectUri => "/linkedin-callback";
  }
}
