using API.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
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

    protected async Task UpdateItemStatusAsync(UIChangeEventArgs e, int itemId)
    {
      ItemListApiModel item = Items.SingleOrDefault(x => x.Id == itemId);

      if (item == null)
      {
        return;
      }

      item.IsDone = (bool)e.Value;

      await AppHttpClient.PutAsync(ApiUrls.UpdateItem.Replace(Urls.UpdateItem, itemId.ToString()), item);
    }

    protected async Task UpdateItemTextAsync(UIChangeEventArgs e, int itemId)
    {
      ItemListApiModel item = Items.SingleOrDefault(x => x.Id == itemId);

      if (item == null)
      {
        return;
      }

      item.Text = (string)e.Value;

      await AppHttpClient.PutAsync(ApiUrls.UpdateItem.Replace(Urls.UpdateItem, itemId.ToString()), item);
    }

    protected async Task DeleteItemAsync(int itemId)
    {
      ItemListApiModel item = Items.SingleOrDefault(x => x.Id == itemId);

      if (item == null)
      {
        return;
      }

      Items.Remove(item);

      await AppHttpClient.DeleteAsync(ApiUrls.DeleteItem.Replace(Urls.DeleteItem, itemId.ToString()));
    }
  }
}
