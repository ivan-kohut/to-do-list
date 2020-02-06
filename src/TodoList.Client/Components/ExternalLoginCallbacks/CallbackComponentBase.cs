using API.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public abstract class CallbackComponentBase : ComponentBase
  {
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    private IAppHttpClient AppHttpClient { get; set; } = null!;

    [Inject]
    private ILocalStorageService LocalStorageService { get; set; } = null!;

    protected abstract string ApiUri { get; }
    protected abstract string? RelativeRedirectUri { get; }

    protected override async Task OnInitializedAsync()
    {
      UserExternalLoginModel model = new UserExternalLoginModel
      {
        Code = await JsRuntime.InvokeAsync<string>("getQueryParameterValue", "code"),
        RedirectUri = string.IsNullOrWhiteSpace(RelativeRedirectUri) 
          ? null 
          : $"{NavigationManager.BaseUri}{RelativeRedirectUri}"
      };

      ApiCallResult<string> loginCallResult = await AppHttpClient.PostAsync<string>(ApiUri, model);

      await LocalStorageService.SetItemAsync(AppState.AuthTokenKey, loginCallResult.Value);

      NavigationManager.NavigateTo(string.Empty);
    }
  }
}
