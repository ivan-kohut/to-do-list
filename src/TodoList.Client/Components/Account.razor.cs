using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TodoList.Client.Components
{
  public class AccountComponent : ComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    protected bool? IsTwoFactorAuthenticationEnabled { get; set; }

    protected override async Task OnInitializedAsync()
    {
      IsTwoFactorAuthenticationEnabled = (await AppHttpClient.GetAsync<bool>(ApiUrls.IsTwoFactorAuthenticationEnabled)).Value;
    }

    protected async Task OnDisableTwoFactorAuthenticationAsync()
    {
      await AppHttpClient.PutAsync(ApiUrls.DisableTwoFactorAuthentication, null);

      NavigationManager.NavigateTo(string.Empty);
    }
  }
}
