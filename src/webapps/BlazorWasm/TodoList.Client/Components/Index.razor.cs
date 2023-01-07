﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoList.Items.API.Models;

namespace TodoList.Client.Components
{
    public class IndexComponent : LoadingSpinnerComponentBase
    {
        [Inject]
        private IAppHttpClient AppHttpClient { get; set; } = null!;

        protected ItemCreateApiModel NewItem { get; set; } = null!;
        protected IList<ItemApiModel> Items { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            NewItem = new ItemCreateApiModel();

            ApiCallResult<IList<ItemApiModel>> apiCallResult = await AppHttpClient.GetAsync<IList<ItemApiModel>>(ApiUrls.GetItemsList);

            Items = apiCallResult.IsSuccess
                ? apiCallResult.Value!
                : throw new Exception("API call is not successful");
        }

        protected override async Task HandleEventAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewItem.Text))
            {
                ApiCallResult<ItemApiModel> itemCreationCallResult = await AppHttpClient
                    .PostAsync<ItemApiModel>(ApiUrls.CreateItem, NewItem);

                Items.Add(
                    itemCreationCallResult.IsSuccess
                        ? itemCreationCallResult.Value!
                        : throw new Exception("API call is not successful")
                );

                NewItem.Text = string.Empty;
            }
        }

        protected async Task UpdateItemStatusAsync(ChangeEventArgs e, ItemApiModel item)
        {
            item.IsDone = e.Value is bool isDone && isDone;

            await UpdateItemAsync(item);
        }

        protected async Task UpdateItemTextAsync(ChangeEventArgs e, ItemApiModel item)
        {
            string? text = e.Value as string;

            if (!string.IsNullOrWhiteSpace(text))
            {
                item.Text = text;

                await UpdateItemAsync(item);
            }
        }

        protected async Task DeleteItemAsync(ItemApiModel item)
        {
            Items.Remove(item);

            await AppHttpClient.DeleteAsync(ApiUrls.DeleteItem.Replace(ApiUrls.IdTemplate, item.Id.ToString()));
        }

        protected async Task MoveUpItemAsync(ItemApiModel item)
        {
            int indexOfItem = Items.IndexOf(item);
            int indexOfPrevItem = indexOfItem - 1;

            await SwapItemsAsync(item, indexOfItem, indexOfPrevItem);
        }

        protected async Task MoveDownItemAsync(ItemApiModel item)
        {
            int indexOfItem = Items.IndexOf(item);
            int indexOfNextItem = indexOfItem + 1;

            await SwapItemsAsync(item, indexOfItem, indexOfNextItem);
        }

        private Task SwapItemsAsync(ItemApiModel item, int indexOfSelectedItem, int indexOfAnotherItem)
        {
            ItemApiModel anotherItem = Items[indexOfAnotherItem];

            Items[indexOfSelectedItem] = anotherItem;
            Items[indexOfAnotherItem] = item;

            int itemPriority = item.Priority;

            item.Priority = anotherItem.Priority;
            anotherItem.Priority = itemPriority;

            return Task.WhenAll(UpdateItemAsync(item), UpdateItemAsync(anotherItem));
        }

        private Task UpdateItemAsync(ItemApiModel item)
        {
            return AppHttpClient.PutAsync(ApiUrls.UpdateItem.Replace(ApiUrls.IdTemplate, item.Id.ToString()), item);
        }
    }
}
