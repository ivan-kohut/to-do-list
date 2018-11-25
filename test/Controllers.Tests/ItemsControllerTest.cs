using Controllers.Models;
using Controllers.Tests.Fixtures;
using Entities;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Repositories;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;
using static Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions;

namespace Controllers.Tests
{
  public class ItemsControllerTest : IClassFixture<TestServerFixture>, IDisposable
  {
    private const string url = "/items";
    private const string mediaType = "application/json";

    private readonly TestServer server;
    private readonly HttpClient client;

    public ItemsControllerTest(TestServerFixture testServerFixture)
    {
      this.server = testServerFixture.Server;
      this.client = testServerFixture.Client;
    }

    [Fact]
    public void All_When_ItemsDoNotExist_Expect_EmptyList()
    {
      HttpResponseMessage response = client.GetAsync(url).Result;

      response.EnsureSuccessStatusCode();

      Assert.Empty(DeserializeResponseBody<IEnumerable<ItemDTO>>(response));
    }

    [Fact]
    public void Save_When_InputModelIsValid_Expect_Saved()
    {
      ItemApiModel itemToSave = new ItemApiModel { Text = "itemText" };

      using (HttpContent httpContent = CreateHttpContent(itemToSave))
      {
        HttpResponseMessage response = client.PostAsync(url, httpContent).Result;

        response.EnsureSuccessStatusCode();

        ItemDTO itemSaved = DeserializeResponseBody<ItemDTO>(response);

        Assert.NotEqual(0, itemSaved.Id);
        Assert.Equal(itemToSave.Text, itemSaved.Text);
      }
    }

    [Fact]
    public void Save_When_InputModelIsNotValid_Expect_BadRequest()
    {
      using (HttpContent httpContent = CreateHttpContent(new ItemApiModel { Text = "" }))
      {
        HttpResponseMessage response = client.PostAsync(url, httpContent).Result;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }
    }

    [Fact]
    public void All_When_ItemsExist_Expect_Returned()
    {
      IEnumerable<Item> items = new List<Item>
      {
        new Item { Text = "firstItemText" },
        new Item { Text = "secondItemText" }
      };

      IEnumerable<ItemDTO> expected = items
        .Select(i => SaveItem(i))
        .ToList();

      HttpResponseMessage response = client.GetAsync(url).Result;

      response.EnsureSuccessStatusCode();

      IEnumerable<ItemDTO> actual = DeserializeResponseBody<IEnumerable<ItemDTO>>(response);

      actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void Update_When_InputModelIsValid_Expect_Updated()
    {
      ItemDTO itemToUpdate = SaveItem(new Item { Text = "itemText" });

      string newItemText = "newItemText";

      using (HttpContent httpContent = CreateHttpContent(new ItemApiModel { Text = newItemText }))
      {
        HttpResponseMessage response = client.PutAsync($"{url}/{itemToUpdate.Id}", httpContent).Result;

        response.EnsureSuccessStatusCode();

        Item itemUpdated = GetAllItems()
          .Where(i => i.Id == itemToUpdate.Id)
          .FirstOrDefault();

        Assert.Equal(newItemText, itemUpdated.Text);
      }
    }

    [Fact]
    public void Update_When_InputModelIsNotValid_Expect_BadRequest()
    {
      using (HttpContent httpContent = CreateHttpContent(new ItemApiModel { Text = "" }))
      {
        HttpResponseMessage response = client.PutAsync($"{url}/{1}", httpContent).Result;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }
    }

    [Fact]
    public void Update_When_ItemIsNotFound_Expect_NotFound()
    {
      using (HttpContent httpContent = CreateHttpContent(new ItemApiModel { Text = "itemText" }))
      {
        HttpResponseMessage response = client.PutAsync($"{url}/{1}", httpContent).Result;

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
      }
    }

    [Fact]
    public void Delete_When_ItemIsFound_Expect_Deleted()
    {
      ItemDTO itemSaved = SaveItem(new Item { Text = "itemText" });

      HttpResponseMessage response = client.DeleteAsync($"{url}/{itemSaved.Id}").Result;

      response.EnsureSuccessStatusCode();

      Item itemDeleted = GetAllItems()
        .Where(i => i.Id == itemSaved.Id)
        .FirstOrDefault();

      Assert.Null(itemDeleted);
    }

    [Fact]
    public void Delete_When_ItemIsNotFound_Expect_NotFound()
    {
      HttpResponseMessage response = client.DeleteAsync($"{url}/{1}").Result;

      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
      AppDbContext appDbContext = GetRepository<AppDbContext>();

      appDbContext.RemoveRange(appDbContext.Items);
      appDbContext.SaveChanges();
    }

    private ItemDTO SaveItem(Item itemToSave)
    {
      AppDbContext appDbContext = GetRepository<AppDbContext>();

      appDbContext.Add(itemToSave);
      appDbContext.SaveChanges();

      return new ItemDTO { Id = itemToSave.Id, Text = itemToSave.Text };
    }

    private IEnumerable<Item> GetAllItems()
    {
      return GetRepository<AppDbContext>().Items.AsNoTracking();
    }

    private T GetRepository<T>()
    {
      return server.Host.Services.GetRequiredService<T>();
    }

    private HttpContent CreateHttpContent(object requestBody)
    {
      return new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, mediaType);
    }

    private T DeserializeResponseBody<T>(HttpResponseMessage response)
    {
      return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
    }
  }
}