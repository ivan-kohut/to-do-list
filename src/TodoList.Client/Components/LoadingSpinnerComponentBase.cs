using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public abstract class LoadingSpinnerComponentBase : ComponentBase
  {
    protected bool IsSpinnerEnabled { get; private set; }

    protected async Task OnEventAsync()
    {
      IsSpinnerEnabled = true;

      await HandleEventAsync();

      IsSpinnerEnabled = false;
    }

    protected abstract Task HandleEventAsync();
  }
}
