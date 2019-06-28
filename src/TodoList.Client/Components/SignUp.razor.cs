using API.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class SignUpComponent : LoadingSpinnerComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; }

    [Inject]
    private IUriHelper UriHelper { get; set; }

    protected UserCreateModel UserCreateModel { get; set; }
    protected IEnumerable<string> Errors { get; set; }

    protected override void OnInit()
    {
      UserCreateModel = new UserCreateModel();
    }

    protected override async Task HandleEventAsync()
    {
      ApiCallResult<object> signUpCallResult = await AppHttpClient.PostAsync<object>(ApiUrls.SignUp, UserCreateModel);

      if (signUpCallResult.IsSuccess)
      {
        UriHelper.NavigateTo("login");
      }
      else
      {
        Errors = signUpCallResult.Errors;
      }
    }
  }
}
