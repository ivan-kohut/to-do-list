using API.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class TwoFactorAuthEnablingComponent : LoadingSpinnerComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; } = null!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    protected ElementReference QrCodeBlock { get; set; }
    protected UserEnableAuthenticatorModel UserEnableAuthenticatorModel { get; set; } = null!;
    protected IEnumerable<string>? Errors { get; set; }

    protected override async Task OnInitializedAsync()
    {
      UserEnableAuthenticatorModel = new UserEnableAuthenticatorModel();

      ApiCallResult<string> apiCallResult = await AppHttpClient.GetAsync<string>(ApiUrls.AuthenticatorUri);

      string authenticatorUri = apiCallResult.IsSuccess
        ? apiCallResult.Value!
        : throw new Exception("API call is not successful");

      await JsRuntime.InvokeAsync<string>("generateQrCode", QrCodeBlock, authenticatorUri);
    }

    protected override async Task HandleEventAsync()
    {
      ApiCallResult enableTwoFactorAuthCallResult = await AppHttpClient
        .PutAsync(ApiUrls.EnableTwoFactorAuthentication, UserEnableAuthenticatorModel);

      if (enableTwoFactorAuthCallResult.IsSuccess)
      {
        NavigationManager.NavigateTo(string.Empty);
      }
      else
      {
        Errors = enableTwoFactorAuthCallResult.Errors;
      }
    }
  }
}
