using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TodoList.Client.Components
{
  public class AccountComponent : ComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; }

    [Inject]
    private IUriHelper UriHelper { get; set; }

    protected bool? IsTwoFactorAuthenticationEnabled { get; set; }

    protected override async Task OnInitAsync()
    {
      IsTwoFactorAuthenticationEnabled = (await AppHttpClient.GetAsync<bool>(ApiUrls.IsTwoFactorAuthenticationEnabled)).Value;
    }

    protected async Task OnDisableTwoFactorAuthenticationAsync()
    {
      await AppHttpClient.PutAsync(ApiUrls.DisableTwoFactorAuthentication, null);

      UriHelper.NavigateTo(string.Empty);
    }
  }
}
