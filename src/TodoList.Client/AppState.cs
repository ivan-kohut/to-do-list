using Blazored.LocalStorage;

namespace TodoList.Client
{
  public class AppState
  {
    public const string AuthTokenKey = "auth-token";
    public const string IndexUrl = "https://localhost:44328";

    private readonly ISyncLocalStorageService localStorageService;

    public AppState(ISyncLocalStorageService localStorageService)
    {
      this.localStorageService = localStorageService;
    }

    public bool IsUserLoggedIn => !string.IsNullOrWhiteSpace(localStorageService.GetItem<string>(AuthTokenKey));
  }
}
