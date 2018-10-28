using Controllers.Models;
using Controllers.Tests.Fixtures;
using FluentAssertions;
using Newtonsoft.Json;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Controllers.Tests
{
  public class ItemsControllerTest : IClassFixture<TestServerFixture>, IDisposable
  {
    private readonly HttpClient client;

    public ItemsControllerTest(TestServerFixture testServerFixture)
    {
      this.client = testServerFixture.Client;
    }

    [Fact]
    public async void All_When_ItemsDoNotExist_Expect_EmptyList()
    {
      HttpResponseMessage response = await client.GetAsync("/items");

      response.EnsureSuccessStatusCode();

      Assert.Empty(await DeserializeResponseBody<IEnumerable<ItemDTO>>(response));
    }

    [Fact]
    public async void Save_When_InputModelIsValid_Expect_Saved()
    {
      ItemApiModel itemToSave = new ItemApiModel { Text = "itemText" };

      using (HttpContent httpContent = new StringContent(SerializeRequestBody(itemToSave), Encoding.UTF8, "application/json"))
      {
        HttpResponseMessage response = await client.PostAsync("/items", httpContent);

        response.EnsureSuccessStatusCode();

        ItemDTO itemSaved = await DeserializeResponseBody<ItemDTO>(response);

        Assert.NotEqual(0, itemSaved.Id);
        Assert.Equal(itemToSave.Text, itemSaved.Text);
      }
    }

    [Fact]
    public async void Save_When_InputModelIsNotValid_Expect_BadRequest()
    {
      ItemApiModel itemToSave = new ItemApiModel { Text = "" };

      using (HttpContent httpContent = new StringContent(SerializeRequestBody(itemToSave), Encoding.UTF8, "application/json"))
      {
        HttpResponseMessage response = await client.PostAsync("/items", httpContent);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }
    }

    [Fact]
    public async void All_When_ItemsExist_Expect_Returned()
    {
      IEnumerable<ItemApiModel> itemApiModels = new List<ItemApiModel>
      {
        new ItemApiModel { Text = "firstItemText" },
        new ItemApiModel { Text = "secondItemText" }
      };

      IEnumerable<ItemDTO> expected = itemApiModels
        .Select(i =>
        {
          using (HttpContent httpContent = new StringContent(SerializeRequestBody(i), Encoding.UTF8, "application/json"))
          {
            return DeserializeResponseBody<ItemDTO>(client.PostAsync("/items", httpContent).Result).Result;
          }
        })
        .ToList();

      HttpResponseMessage response = await client.GetAsync("/items");

      response.EnsureSuccessStatusCode();

      IEnumerable<ItemDTO> actual = await DeserializeResponseBody<IEnumerable<ItemDTO>>(response);

      actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async void Update_When_InputModelIsValid_Expect_Updated()
    {
      ItemApiModel itemToSave = new ItemApiModel { Text = "itemText" };

      ItemDTO itemToUpdate;

      using (HttpContent httpContent = new StringContent(SerializeRequestBody(itemToSave), Encoding.UTF8, "application/json"))
      {
        itemToUpdate = DeserializeResponseBody<ItemDTO>(client.PostAsync("/items", httpContent).Result).Result;
      }

      string newItemText = "newItemText";

      using (HttpContent httpContent = new StringContent(SerializeRequestBody(new ItemApiModel { Text = newItemText }), Encoding.UTF8, "application/json"))
      {
        HttpResponseMessage response = await client.PutAsync($"/items/{itemToUpdate.Id}", httpContent);

        response.EnsureSuccessStatusCode();

        ItemDTO itemUpdated = DeserializeResponseBody<IEnumerable<ItemDTO>>(client.GetAsync("/items").Result).Result
          .Where(i => i.Id == itemToUpdate.Id)
          .FirstOrDefault();

        Assert.Equal(newItemText, itemUpdated.Text);
      }
    }

    [Fact]
    public async void Update_When_InputModelIsNotValid_Expect_BadRequest()
    {
      ItemApiModel itemToUpdate = new ItemApiModel { Text = "" };

      using (HttpContent httpContent = new StringContent(SerializeRequestBody(itemToUpdate), Encoding.UTF8, "application/json"))
      {
        HttpResponseMessage response = await client.PutAsync($"/items/{1}", httpContent);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }
    }

    [Fact]
    public async void Update_When_ItemIsNotFound_Expect_NotFound()
    {
      ItemApiModel itemToUpdate = new ItemApiModel { Text = "itemText" };

      using (HttpContent httpContent = new StringContent(SerializeRequestBody(itemToUpdate), Encoding.UTF8, "application/json"))
      {
        HttpResponseMessage response = await client.PutAsync($"/items/{1}", httpContent);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
      }
    }

    [Fact]
    public async void Delete_When_ItemIsFound_Expect_Deleted()
    {
      ItemApiModel itemToSave = new ItemApiModel { Text = "itemText" };

      ItemDTO itemSaved;

      using (HttpContent httpContent = new StringContent(SerializeRequestBody(itemToSave), Encoding.UTF8, "application/json"))
      {
        itemSaved = DeserializeResponseBody<ItemDTO>(client.PostAsync("/items", httpContent).Result).Result;
      }

      HttpResponseMessage response = await client.DeleteAsync($"/items/{itemSaved.Id}");

      response.EnsureSuccessStatusCode();

      ItemDTO itemDeleted = DeserializeResponseBody<IEnumerable<ItemDTO>>(client.GetAsync("/items").Result).Result
        .Where(i => i.Id == itemSaved.Id)
        .FirstOrDefault();

      Assert.Null(itemDeleted);
    }

    [Fact]
    public async void Delete_When_ItemIsNotFound_Expect_NotFound()
    {
      HttpResponseMessage response = await client.DeleteAsync($"/items/{1}");

      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
      foreach (ItemDTO item in DeserializeResponseBody<IEnumerable<ItemDTO>>(client.GetAsync("/items").Result).Result)
      {
        HttpResponseMessage response = client.DeleteAsync($"/items/{item.Id}").Result;
      }
    }

    private string SerializeRequestBody(object requestBody)
    {
      return JsonConvert.SerializeObject(requestBody);
    }

    private async Task<T> DeserializeResponseBody<T>(HttpResponseMessage response)
    {
      return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
    }
  }
}
