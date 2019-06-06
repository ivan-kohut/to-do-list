using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Layouts;
using System.Linq;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class MainLayoutComponent : LayoutComponentBase
  {
    [Inject]
    private AppState AppState { get; set; }

    [Inject]
    private IUriHelper UriHelper { get; set; }

    protected async override Task OnInitAsync()
    {
      if (!AppState.IsUserLoggedIn && new string(UriHelper.GetAbsoluteUri().Except(UriHelper.GetBaseUri()).ToArray()) != "login")
      {
        UriHelper.NavigateTo("login");
      }
    }
  }
}
