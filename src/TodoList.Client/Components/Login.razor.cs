using API.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class LoginComponent : ComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; }

    [Inject]
    private ILocalStorageService LocalStorageService { get; set; }

    [Inject]
    private IUriHelper UriHelper { get; set; }

    protected UserLoginModel UserLoginModel { get; set; }
    protected bool IsTwoFactorTokenFieldDisplayed { get; set; }
    protected IEnumerable<string> Errors { get; set; }

    protected override void OnInit()
    {
      UserLoginModel = new UserLoginModel();
    }

    protected async Task OnLoginAsync()
    {
      ApiCallResult<string> loginCallResult = await AppHttpClient.PostAsync<string>(
        "https://localhost:44388/api/v1/users/login",
        UserLoginModel
      );

      if (loginCallResult.IsSuccess)
      {
        await LocalStorageService.SetItemAsync(AppState.AuthTokenKey, loginCallResult.Value);

        UriHelper.NavigateTo(string.Empty);
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
