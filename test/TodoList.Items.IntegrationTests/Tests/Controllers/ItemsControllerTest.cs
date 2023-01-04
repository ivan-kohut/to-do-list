using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TodoList.Items.API.Models;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Infrastructure;
using TodoList.Items.IntegrationTests.Extensions;
using TodoList.Items.IntegrationTests.Fixtures;
using Xunit;

namespace TodoList.Items.IntegrationTests.Tests.Controllers
{
  [Collection(nameof(IntegrationTestCollection))]
  public class ItemsControllerTest : ControllerTestBase, IDisposable
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

        (await DeserializeResponseBodyAsync<IEnumerable<ItemApiModel>>(response)).Should().BeEmpty();

        Server.Services.GetRequiredService<IMemoryCache>().Remove(IdentityId);
      }

      [Fact]
      public async Task When_ItemsExist_Expect_Returned()
      {
        IEnumerable<Item> items = new List<Item>
        {
          new Item(UserId, "firstItemText", 2, ItemStatus.Todo),
          new Item(UserId, "secondItemText", 1, ItemStatus.Done)
        };

        IEnumerable<ItemApiModel> expected = (await Task.WhenAll(items.Select(i => SaveItemAsync(i))))
          .OrderBy(i => i.Priority)
          .ToList();

        // Act
        HttpResponseMessage response = await GetAsync(url);

        response.EnsureSuccessStatusCode();

        IEnumerable<ItemApiModel> actual = await DeserializeResponseBodyAsync<IEnumerable<ItemApiModel>>(response);

        actual.Should().BeEquivalentTo(expected);

        Server.Services.GetRequiredService<IMemoryCache>().Remove(IdentityId);
      }

      [Fact]
      public async Task When_ItemsExistInCache_Expect_ReturnedFromCache()
      {
        IEnumerable<Item> items = new List<Item>
        {
          new Item(UserId, "firstItemText", 2, ItemStatus.Todo),
          new Item(UserId, "secondItemText", 1, ItemStatus.Done)
        };

        IEnumerable<ItemApiModel> expected = (await Task.WhenAll(items.Select(i => SaveItemAsync(i))))
          .OrderBy(i => i.Priority)
          .ToList();

        HttpResponseMessage response = await GetAsync(url);

        response.EnsureSuccessStatusCode();

        IEnumerable<ItemApiModel> actualFromDb = await DeserializeResponseBodyAsync<IEnumerable<ItemApiModel>>(response);

        actualFromDb.Should().BeEquivalentTo(expected);

        await SaveItemAsync(new Item(UserId, "thirdItemText", 3, ItemStatus.Todo));

        // Act
        response = await GetAsync(url);

        response.EnsureSuccessStatusCode();

        IEnumerable<ItemApiModel> actualFromCache = await DeserializeResponseBodyAsync<IEnumerable<ItemApiModel>>(response);

        actualFromCache.Should().BeEquivalentTo(expected);

        Server.Services.GetRequiredService<IMemoryCache>().Remove(IdentityId);
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

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
      }

      [Fact]
      public async Task When_UserIsNotFound_Expect_NotFound()
      {
        int newIdentityId = 99;

        await UpdateIdentityIdAsync(IdentityId, newIdentityId);

        try
        {
          // Act
          HttpResponseMessage response = await PostAsync(url, new ItemCreateApiModel { Text = "itemText" });

          response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
          await UpdateIdentityIdAsync(newIdentityId, IdentityId);
        }
      }

      [Fact]
      public async Task When_InputModelIsValid_Expect_Saved()
      {
        ItemCreateApiModel itemToSave = new() { Text = "itemText" };

        // Act
        HttpResponseMessage response = await PostAsync(url, itemToSave);

        response.EnsureSuccessStatusCode();

        ItemApiModel itemSaved = await DeserializeResponseBodyAsync<ItemApiModel>(response);

        ItemApiModel expectedItem = new()
        {
          IsDone = false,
          Text = itemToSave.Text,
          Priority = 1
        };

        itemSaved.Should().BeEquivalentTo(expectedItem, o => o.Excluding(m => m.Id));
        itemSaved.Id.Should().NotBe(default);
      }
    }

    public class PutItemAsync : ItemsControllerTest
    {
      public PutItemAsync(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_InputModelIsNotValid_Expect_BadRequest()
      {
        ItemApiModel itemToUpdate = new()
        {
          Id = 1,
          Text = string.Empty,
          Priority = 10
        };

        // Act
        HttpResponseMessage response = await PutAsync($"{url}/{itemToUpdate.Id}", itemToUpdate);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
      }

      [Fact]
      public async Task When_IdFromRequestPathDoesNotEqualToIdFromModel_Expect_BadRequest()
      {
        ItemApiModel itemToUpdate = new()
        {
          Id = 2,
          Text = "itemText"
        };

        // Act
        HttpResponseMessage response = await PutAsync($"{url}/{1}", itemToUpdate);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
      }

      [Fact]
      public async Task When_ItemIsNotFound_Expect_NotFound()
      {
        ItemApiModel itemToUpdate = new()
        {
          Id = 1,
          Text = "itemText"
        };

        // Act
        HttpResponseMessage response = await PutAsync($"{url}/{itemToUpdate.Id}", itemToUpdate);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
      }

      [Fact]
      public async Task When_InputModelIsValid_Expect_Updated()
      {
        ItemApiModel itemToUpdate = await SaveItemAsync(
          new Item(UserId, "itemText", 10, ItemStatus.Todo)
        );

        itemToUpdate.IsDone = true;
        itemToUpdate.Text = "newItemText";
        itemToUpdate.Priority = 25;

        // Act
        HttpResponseMessage response = await PutAsync($"{url}/{itemToUpdate.Id}", itemToUpdate);

        response.EnsureSuccessStatusCode();

        Item itemUpdated = (await GetAllItemsAsync())
          .Single(i => i.Id == itemToUpdate.Id);

        itemUpdated.Status.Should().BeEquivalentTo(ItemStatus.Done);
        itemUpdated.Text.Should().BeEquivalentTo(itemToUpdate.Text);
        itemUpdated.Priority.Should().Be(itemToUpdate.Priority);
      }
    }

    public class DeleteItemAsync : ItemsControllerTest
    {
      public DeleteItemAsync(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_ItemIsNotFound_Expect_NotFound()
      {
        // Act
        HttpResponseMessage response = await DeleteAsync($"{url}/{1}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
      }

      [Fact]
      public async Task When_ItemIsFound_Expect_Deleted()
      {
        ItemApiModel itemSaved = await SaveItemAsync(new Item(UserId, "itemText", 1, ItemStatus.Todo));

        // Act
        HttpResponseMessage response = await DeleteAsync($"{url}/{itemSaved.Id}");

        response.EnsureSuccessStatusCode();

        Item? itemDeleted = (await GetAllItemsAsync())
          .SingleOrDefault(i => i.Id == itemSaved.Id);

        itemDeleted.Should().BeNull();
      }
    }

    public void Dispose()
    {
      using IServiceScope scope = Server.CreateScope();

      ItemsDbContext itemsDbContext = scope.ServiceProvider.GetRequiredService<ItemsDbContext>();

      itemsDbContext.Rollback<Item>();
      itemsDbContext.SaveChanges();

      GC.SuppressFinalize(this);
    }

    private async Task<IEnumerable<Item>> GetAllItemsAsync()
    {
      using IServiceScope scope = Server.CreateScope();

      ItemsDbContext itemsDbContext = scope.ServiceProvider.GetRequiredService<ItemsDbContext>();

      return await itemsDbContext
        .Items
        .Include(i => i.Status)
        .ToListAsync();
    }

    private async Task UpdateIdentityIdAsync(int fromIdentityId, int toIdentityId)
    {
      using IServiceScope scope = Server.CreateScope();

      ItemsDbContext itemsDbContext = scope.ServiceProvider.GetRequiredService<ItemsDbContext>();

      User? user = await itemsDbContext
        .Users
        .SingleOrDefaultAsync(u => u.IdentityId == fromIdentityId);

      if (user != default)
      {
        user.SetIdentityId(toIdentityId);

        await itemsDbContext.SaveChangesAsync();
      }
    }
  }
}
