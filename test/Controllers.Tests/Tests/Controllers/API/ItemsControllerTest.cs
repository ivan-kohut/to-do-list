using Controllers.Models;
using Controllers.Tests.Extensions;
using Controllers.Tests.Fixtures;
using Entities;
using FluentAssertions;
using Repositories;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace Controllers.Tests
{
  [Collection(nameof(IntegrationTestCollection))]
  public class ItemsControllerTest : ApiControllerTestBase, IDisposable
  {
    private const string url = "/items";

    public ItemsControllerTest(TestServerFixture testServerFixture) : base(testServerFixture)
    {
    }

    [Fact]
    public void All_When_ItemsDoNotExist_Expect_EmptyList()
    {
      // Act
      HttpResponseMessage response = Get(url);

      response.EnsureSuccessStatusCode();

      Assert.Empty(DeserializeResponseBody<IEnumerable<ItemDTO>>(response));
    }

    [Fact]
    public void Save_When_InputModelIsValid_Expect_Saved()
    {
      ItemApiModel itemToSave = new ItemApiModel { Text = "itemText" };

      // Act
      HttpResponseMessage response = Post(url, itemToSave);

      response.EnsureSuccessStatusCode();

      ItemDTO itemSaved = DeserializeResponseBody<ItemDTO>(response);

      Assert.NotEqual(0, itemSaved.Id);
      Assert.Equal(itemToSave.Text, itemSaved.Text);
    }

    [Fact]
    public void Save_When_InputModelIsNotValid_Expect_BadRequest()
    {
      // Act
      HttpResponseMessage response = Post(url, new ItemApiModel { Text = "" });

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

      // Act
      HttpResponseMessage response = Get(url);

      response.EnsureSuccessStatusCode();

      IEnumerable<ItemDTO> actual = DeserializeResponseBody<IEnumerable<ItemDTO>>(response);

      actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void Update_When_InputModelIsValid_Expect_Updated()
    {
      ItemDTO itemToUpdate = SaveItem(new Item { Text = "itemText" });

      string newItemText = "newItemText";

      // Act
      HttpResponseMessage response = Put($"{url}/{itemToUpdate.Id}", new ItemApiModel { Text = newItemText });

      response.EnsureSuccessStatusCode();

      Item itemUpdated = GetAllItems()
        .Where(i => i.Id == itemToUpdate.Id)
        .FirstOrDefault();

      Assert.Equal(newItemText, itemUpdated.Text);
    }

    [Fact]
    public void Update_When_InputModelIsNotValid_Expect_BadRequest()
    {
      // Act
      HttpResponseMessage response = Put($"{url}/{1}", new ItemApiModel { Text = "" });

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void Update_When_ItemIsNotFound_Expect_NotFound()
    {
      // Act
      HttpResponseMessage response = Put($"{url}/{1}", new ItemApiModel { Text = "itemText" });

      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void Delete_When_ItemIsFound_Expect_Deleted()
    {
      ItemDTO itemSaved = SaveItem(new Item { Text = "itemText" });

      // Act
      HttpResponseMessage response = Delete($"{url}/{itemSaved.Id}");

      response.EnsureSuccessStatusCode();

      Item itemDeleted = GetAllItems()
        .Where(i => i.Id == itemSaved.Id)
        .FirstOrDefault();

      Assert.Null(itemDeleted);
    }

    [Fact]
    public void Delete_When_ItemIsNotFound_Expect_NotFound()
    {
      // Act
      HttpResponseMessage response = Delete($"{url}/{1}");

      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
      using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
      {
        appDbContext.Rollback<Item>();

        appDbContext.SaveChanges();
      }
    }

    private ItemDTO SaveItem(Item itemToSave)
    {
      using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
      {
        appDbContext.Add(itemToSave);
        appDbContext.SaveChanges();

        return new ItemDTO { Id = itemToSave.Id, Text = itemToSave.Text };
      }
    }

    private IEnumerable<Item> GetAllItems()
    {
      using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
      {
        return Server.GetService<AppDbContext>().Items;
      }
    }
  }
}