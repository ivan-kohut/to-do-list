using API.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class SignUpComponent : LoadingSpinnerComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    protected UserCreateModel UserCreateModel { get; set; } = null!;
    protected IEnumerable<string>? Errors { get; set; }

    protected override void OnInitialized()
    {
      UserCreateModel = new UserCreateModel();
    }

    protected override async Task HandleEventAsync()
    {
      ApiCallResult<object> signUpCallResult = await AppHttpClient.PostAsync<object>(ApiUrls.SignUp, UserCreateModel);

      if (signUpCallResult.IsSuccess)
      {
        NavigationManager.NavigateTo("login");
      }
      else
      {
        Errors = signUpCallResult.Errors;
      }
    }
  }
}
