using Microsoft.AspNetCore.Components;

namespace TodoList.Client.Components
{
  public abstract class CallbackComponentBase : ComponentBase
  {
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    protected abstract string ApiUri { get; }
    protected abstract string? RelativeRedirectUri { get; }

    protected override void OnInitialized()
    {
      NavigationManager.NavigateTo(string.Empty);
    }
  }
}
