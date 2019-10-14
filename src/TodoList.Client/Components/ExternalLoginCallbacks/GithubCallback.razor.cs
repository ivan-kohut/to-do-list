namespace TodoList.Client.Components
{
  public class GithubCallbackComponent : CallbackComponentBase
  {
    protected override string ApiUri => ApiUrls.LoginByGithub;

    protected override string? RelativeRedirectUri => null;
  }
}
