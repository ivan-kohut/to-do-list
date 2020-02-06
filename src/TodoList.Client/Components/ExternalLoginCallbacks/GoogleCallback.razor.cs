namespace TodoList.Client.Components
{
  public class GoogleCallbackComponent : CallbackComponentBase
  {
    protected override string ApiUri => ApiUrls.LoginByGoogle;

    protected override string? RelativeRedirectUri => "google-callback";
  }
}
