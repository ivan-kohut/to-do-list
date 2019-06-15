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

    protected async Task UpdateItemStatusAsync(UIChangeEventArgs e, ItemListApiModel item)
    {
      item.IsDone = (bool)e.Value;

      await UpdateItemAsync(item);
    }

    protected async Task UpdateItemTextAsync(UIChangeEventArgs e, ItemListApiModel item)
    {
      item.Text = (string)e.Value;

      await UpdateItemAsync(item);
    }

    protected async Task DeleteItemAsync(ItemListApiModel item)
    {
      Items.Remove(item);

      await AppHttpClient.DeleteAsync(ApiUrls.DeleteItem.Replace(Urls.DeleteItem, item.Id.ToString()));
    }

    protected async Task MoveUpItemAsync(ItemListApiModel item)
    {
      int indexOfItem = Items.IndexOf(item);
      int indexOfPrevItem = indexOfItem - 1;

      await SwapItemsAsync(item, indexOfItem, indexOfPrevItem);
    }

    protected async Task MoveDownItemAsync(ItemListApiModel item)
    {
      int indexOfItem = Items.IndexOf(item);
      int indexOfNextItem = indexOfItem + 1;

      await SwapItemsAsync(item, indexOfItem, indexOfNextItem);
    }

    private Task SwapItemsAsync(ItemListApiModel item, int indexOfSelectedItem, int indexOfAnotherItem)
    {
      ItemListApiModel anotherItem = Items[indexOfAnotherItem];

      Items[indexOfSelectedItem] = anotherItem;
      Items[indexOfAnotherItem] = item;

      int itemPriority = item.Priority;

      item.Priority = anotherItem.Priority;
      anotherItem.Priority = itemPriority;

      return Task.WhenAll(UpdateItemAsync(item), UpdateItemAsync(anotherItem));
    }

    private Task UpdateItemAsync(ItemListApiModel item)
    {
      return AppHttpClient.PutAsync(ApiUrls.UpdateItem.Replace(Urls.UpdateItem, item.Id.ToString()), item);
    }
  }
}
