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

    protected ItemCreateApiModel NewItem { get; set; }
    protected IList<ItemListApiModel> Items { get; set; }

    protected override async Task OnInitAsync()
    {
      NewItem = new ItemCreateApiModel();
      Items = (await AppHttpClient.GetAsync<IList<ItemListApiModel>>(ApiUrls.GetItemsList)).Value;
    }

    protected async Task OnCreateItemAsync()
    {
      if (!string.IsNullOrWhiteSpace(NewItem.Text))
      {
        ApiCallResult<ItemListApiModel> itemCreationCallResult = await AppHttpClient
          .PostAsync<ItemListApiModel>(ApiUrls.CreateItem, NewItem);

        Items.Add(itemCreationCallResult.Value);
        NewItem.Text = string.Empty;
      }
    }
  }
}
