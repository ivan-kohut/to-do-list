using Models;
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
    private const string url = "/api/v1/items";

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
    public void All_When_ItemsExist_Expect_Returned()
    {
      IEnumerable<Item> items = new List<Item>
      {
        new Item { UserId = UserId, Text = "firstItemText", Priority = 2, Status = ItemStatus.Todo },
        new Item { UserId = UserId, Text = "secondItemText", Priority = 1, Status = ItemStatus.Done }
      };

      IEnumerable<ItemDTO> expected = items
        .Select(i => SaveItem(i))
        .OrderBy(i => i.Priority)
        .ToList();

      // Act
      HttpResponseMessage response = Get(url);

      response.EnsureSuccessStatusCode();

      IEnumerable<ItemDTO> actual = DeserializeResponseBody<IEnumerable<ItemDTO>>(response);

      actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void Save_When_InputModelIsNotValid_Expect_BadRequest()
    {
      // Act
      HttpResponseMessage response = Post(url, new ItemCreateApiModel { Text = "" });

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void Save_When_InputModelIsValid_Expect_Saved()
    {
      ItemCreateApiModel itemToSave = new ItemCreateApiModel { Text = "itemText" };

      // Act
      HttpResponseMessage response = Post(url, itemToSave);

      response.EnsureSuccessStatusCode();

      ItemDTO itemSaved = DeserializeResponseBody<ItemDTO>(response);

      Assert.NotEqual(0, itemSaved.Id);
      Assert.Equal(itemToSave.Text, itemSaved.Text);
      Assert.Equal(1, itemSaved.Priority);
      Assert.Equal((int)ItemStatus.Todo, itemSaved.StatusId);
    }

    [Fact]
    public void UpdatePartially_When_InputModelIsNotValid_Expect_BadRequest()
    {
      // Act
      HttpResponseMessage response = Patch($"{url}/{1}", null);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void UpdatePartially_When_ItemIsNotFound_Expect_NotFound()
    {
      // Act
      HttpResponseMessage response = Patch($"{url}/{1}", Enumerable.Empty<PatchDTO>());

      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void UpdatePartially_When_InputModelIsValid_Expect_Updated()
    {
      ItemDTO itemToUpdate = SaveItem(new Item { UserId = UserId, Text = "itemText", Priority = 1, Status = ItemStatus.Todo });

      PatchDTO textPatchDTO = new PatchDTO
      {
        Name = "Text",
        Value = "newItemText"
      };

      PatchDTO priorityPatchDTO = new PatchDTO
      {
        Name = "Priority",
        Value = 2
      };

      PatchDTO statusPatchDTO = new PatchDTO
      {
        Name = "StatusId",
        Value = 2
      };

      // Act
      HttpResponseMessage response = Patch($"{url}/{itemToUpdate.Id}", new List<PatchDTO> { textPatchDTO, priorityPatchDTO, statusPatchDTO });

      response.EnsureSuccessStatusCode();

      Item itemUpdated = GetAllItems()
        .Single(i => i.Id == itemToUpdate.Id);

      Assert.Equal(textPatchDTO.Value, itemUpdated.Text);
      Assert.Equal(priorityPatchDTO.Value, itemUpdated.Priority);
      Assert.Equal(statusPatchDTO.Value, (int)itemUpdated.Status);
    }

    [Fact]
    public void Delete_When_ItemIsNotFound_Expect_NotFound()
    {
      // Act
      HttpResponseMessage response = Delete($"{url}/{1}");

      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void Delete_When_ItemIsFound_Expect_Deleted()
    {
      ItemDTO itemSaved = SaveItem(new Item { UserId = UserId, Text = "itemText", Priority = 1, Status = ItemStatus.Todo });

      // Act
      HttpResponseMessage response = Delete($"{url}/{itemSaved.Id}");

      response.EnsureSuccessStatusCode();

      Item itemDeleted = GetAllItems()
        .SingleOrDefault(i => i.Id == itemSaved.Id);

      Assert.Null(itemDeleted);
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

        return new ItemDTO
        {
          Id = itemToSave.Id,
          Text = itemToSave.Text,
          StatusId = (int)itemToSave.Status,
          Priority = itemToSave.Priority
        };
      }
    }

    private IEnumerable<Item> GetAllItems()
    {
      using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
      {
        return appDbContext
          .Items
          .ToList();
      }
    }
  }
}