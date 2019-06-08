using Blazored.LocalStorage;

namespace TodoList.Client
{
  public class AppState
  {
    public const string AuthTokenKey = "auth-token";

    private readonly ISyncLocalStorageService localStorageService;

    public AppState(ISyncLocalStorageService localStorageService)
    {
      this.localStorageService = localStorageService;
    }

    public bool IsUserLoggedIn => !string.IsNullOrWhiteSpace(localStorageService.GetItem<string>(AuthTokenKey));
  }
}
