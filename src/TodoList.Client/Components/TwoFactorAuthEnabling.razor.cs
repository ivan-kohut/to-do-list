using API.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class TwoFactorAuthEnablingComponent : ComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; }

    [Inject]
    private IJSRuntime JsRuntime { get; set; }

    [Inject]
    private IUriHelper UriHelper { get; set; }

    protected ElementRef QrCodeBlock { get; set; }
    protected UserEnableAuthenticatorModel UserEnableAuthenticatorModel { get; set; }
    protected IEnumerable<string> Errors { get; set; }

    protected override async Task OnInitAsync()
    {
      UserEnableAuthenticatorModel = new UserEnableAuthenticatorModel();

      string authenticatorUri = (await AppHttpClient.GetAsync<string>(ApiUrls.AuthenticatorUri)).Value;

      await JsRuntime.InvokeAsync<string>("generateQrCode", QrCodeBlock, authenticatorUri);
    }

    protected async Task OnEnableTwoFactorAuthenticationAsync()
    {
      ApiCallResult enableTwoFactorAuthCallResult = await AppHttpClient
        .PutAsync(ApiUrls.EnableTwoFactorAuthentication, UserEnableAuthenticatorModel);

      if (enableTwoFactorAuthCallResult.IsSuccess)
      {
        UriHelper.NavigateTo(string.Empty);
      }
      else
      {
        Errors = enableTwoFactorAuthCallResult.Errors;
      }
    }
  }
}
