using API.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class LoginComponent : LoadingSpinnerComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; }

    [Inject]
    private ILocalStorageService LocalStorageService { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    protected UserLoginModel UserLoginModel { get; set; }
    protected bool IsTwoFactorTokenFieldDisplayed { get; set; }
    protected IEnumerable<string> Errors { get; set; }

    protected override void OnInitialized()
    {
      UserLoginModel = new UserLoginModel();
    }

    protected override async Task HandleEventAsync()
    {
      ApiCallResult<string> loginCallResult = await AppHttpClient.PostAsync<string>(ApiUrls.Login, UserLoginModel);

      if (loginCallResult.IsSuccess)
      {
        await LocalStorageService.SetItemAsync(AppState.AuthTokenKey, loginCallResult.Value);

        NavigationManager.NavigateTo(string.Empty);
      }
      else
      {
        if (loginCallResult.StatusCode == 427)
        {
          IsTwoFactorTokenFieldDisplayed = true;
        }

        Errors = loginCallResult.Errors;
      }
    }
  }
}
