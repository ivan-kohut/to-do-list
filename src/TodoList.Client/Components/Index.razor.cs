using API.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class IndexComponent : ComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; }

    protected IList<ItemListApiModel> Items { get; set; }

    protected override async Task OnInitAsync()
    {
      Items = (await AppHttpClient.GetAsync<IList<ItemListApiModel>>(ApiUrls.GetItemsList)).Value;
    }
  }
}
