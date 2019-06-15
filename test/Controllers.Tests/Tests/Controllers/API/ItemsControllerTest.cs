using API.Models;
using Controllers.Tests.Extensions;
using Controllers.Tests.Fixtures;
using Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Controllers.Tests
{
  [Collection(nameof(IntegrationTestCollection))]
  public class ItemsControllerTest : ApiControllerTestBase, IDisposable
  {
    private const string url = "/api/v1/items";

    public ItemsControllerTest(TestServerFixture testServerFixture) : base(testServerFixture)
    {
    }

    public class GetAllAsync : ItemsControllerTest
    {
      public GetAllAsync(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_ItemsDoNotExist_Expect_EmptyList()
      {
        // Act
        HttpResponseMessage response = await GetAsync(url);

        response.EnsureSuccessStatusCode();

        Assert.Empty(await DeserializeResponseBodyAsync<IEnumerable<ItemApiModel>>(response));
      }

      [Fact]
      public async Task When_ItemsExist_Expect_Returned()
      {
        IEnumerable<Item> items = new List<Item>
        {
          new Item { UserId = UserId, Text = "firstItemText", Priority = 2, Status = ItemStatus.Todo },
          new Item { UserId = UserId, Text = "secondItemText", Priority = 1, Status = ItemStatus.Done }
        };

        IEnumerable<ItemApiModel> expected = (await Task.WhenAll(items.Select(i => SaveItemAsync(i))))
          .OrderBy(i => i.Priority)
          .ToList();

        // Act
        HttpResponseMessage response = await GetAsync(url);

        response.EnsureSuccessStatusCode();

        IEnumerable<ItemApiModel> actual = await DeserializeResponseBodyAsync<IEnumerable<ItemApiModel>>(response);

        actual.ShouldBeEquivalentTo(expected);
      }
    }

    public class SaveAsync : ItemsControllerTest
    {
      public SaveAsync(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_InputModelIsNotValid_Expect_BadRequest()
      {
        // Act
        HttpResponseMessage response = await PostAsync(url, new ItemCreateApiModel { Text = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_InputModelIsValid_Expect_Saved()
      {
        ItemCreateApiModel itemToSave = new ItemCreateApiModel { Text = "itemText" };

        // Act
        HttpResponseMessage response = await PostAsync(url, itemToSave);

        response.EnsureSuccessStatusCode();

        ItemApiModel itemSaved = await DeserializeResponseBodyAsync<ItemApiModel>(response);

        Assert.NotEqual(0, itemSaved.Id);
        Assert.Equal(itemToSave.Text, itemSaved.Text);
        Assert.Equal(1, itemSaved.Priority);
        Assert.False(itemSaved.IsDone);
      }
    }

    public class PutAsync : ItemsControllerTest
    {
      public PutAsync(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_InputModelIsNotValid_Expect_BadRequest()
      {
        ItemApiModel itemToUpdate = new ItemApiModel
        {
          Id = 1,
          Text = string.Empty,
          Priority = 10
        };

        // Act
        HttpResponseMessage response = await PutAsync($"{url}/{itemToUpdate.Id}", itemToUpdate);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_IdFromRequestPathDoesNotEqualToIdFromModel_Expect_BadRequest()
      {
        ItemApiModel itemToUpdate = new ItemApiModel
        {
          Id = 2,
          Text = "itemText"
        };

        // Act
        HttpResponseMessage response = await PutAsync($"{url}/{1}", itemToUpdate);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_ItemIsNotFound_Expect_NotFound()
      {
        ItemApiModel itemToUpdate = new ItemApiModel
        {
          Id = 1,
          Text = "itemText"
        };

        // Act
        HttpResponseMessage response = await PutAsync($"{url}/{itemToUpdate.Id}", itemToUpdate);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
      }

      [Fact]
      public async Task When_InputModelIsValid_Expect_Updated()
      {
        ItemApiModel itemToUpdate = await SaveItemAsync(
          new Item { UserId = UserId, Text = "itemText", Priority = 10, Status = ItemStatus.Todo }
        );

        itemToUpdate.IsDone = true;
        itemToUpdate.Text = "newItemText";
        itemToUpdate.Priority = 25;

        // Act
        HttpResponseMessage response = await PutAsync($"{url}/{itemToUpdate.Id}", itemToUpdate);

        response.EnsureSuccessStatusCode();

        Item itemUpdated = (await GetAllItemsAsync())
          .Single(i => i.Id == itemToUpdate.Id);

        Assert.Equal(ItemStatus.Done, itemUpdated.Status);
        Assert.Equal(itemToUpdate.Text, itemUpdated.Text);
        Assert.Equal(itemToUpdate.Priority, itemUpdated.Priority);
      }
    }

    public class DeleteAsync : ItemsControllerTest
    {
      public DeleteAsync(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_ItemIsNotFound_Expect_NotFound()
      {
        // Act
        HttpResponseMessage response = await DeleteAsync($"{url}/{1}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
      }

      [Fact]
      public async Task When_ItemIsFound_Expect_Deleted()
      {
        ItemApiModel itemSaved = await SaveItemAsync(new Item { UserId = UserId, Text = "itemText", Priority = 1, Status = ItemStatus.Todo });

        // Act
        HttpResponseMessage response = await DeleteAsync($"{url}/{itemSaved.Id}");

        response.EnsureSuccessStatusCode();

        Item itemDeleted = (await GetAllItemsAsync())
          .SingleOrDefault(i => i.Id == itemSaved.Id);

        Assert.Null(itemDeleted);
      }
    }

    public void Dispose()
    {
      using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
      {
        appDbContext.Rollback<Item>();

        appDbContext.SaveChanges();
      }
    }

    private async Task<IEnumerable<Item>> GetAllItemsAsync()
    {
      using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
      {
        return await appDbContext
          .Items
          .ToListAsync();
      }
    }
  }
}
