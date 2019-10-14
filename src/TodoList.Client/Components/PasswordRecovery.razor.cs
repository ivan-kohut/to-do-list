using API.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class PasswordRecoveryComponent : LoadingSpinnerComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; } = null!;

    protected UserForgotPasswordModel UserForgotPasswordModel { get; set; } = null!;
    protected bool IsPasswordRecovered { get; set; }
    protected IEnumerable<string>? Errors { get; set; }

    protected override void OnInitialized()
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
