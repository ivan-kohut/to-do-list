using API.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class PasswordRecoveryComponent : LoadingSpinnerComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; }

    protected UserForgotPasswordModel UserForgotPasswordModel { get; set; }
    protected bool IsPasswordRecovered { get; set; }
    protected IEnumerable<string> Errors { get; set; }

    protected override void OnInit()
    {
      UserForgotPasswordModel = new UserForgotPasswordModel();
    }

    protected override async Task HandleEventAsync()
    {
      ApiCallResult<object> passwordRecoveryCallResult = await AppHttpClient.PostAsync<object>(ApiUrls.PasswordRecovery, UserForgotPasswordModel);

      if (passwordRecoveryCallResult.IsSuccess)
      {
        IsPasswordRecovered = true;
      }
      else
      {
        Errors = passwordRecoveryCallResult.Errors;
      }
    }
  }
}
