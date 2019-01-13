using Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using Repositories;
using Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Services.Tests
{
  public class ItemServiceTest
  {
    private readonly Mock<IItemRepository> mockItemRepository;

    private readonly ItemService itemService;

    public ItemServiceTest()
    {
      this.mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);

      this.itemService = new ItemService(mockItemRepository.Object);
    }

    [Fact]
    public void AllAsync_When_ItemsDoNotExist_Expect_EmptyList()
    {
      mockItemRepository
        .Setup(r => r.All())
        .Returns(new List<Item>().AsQueryable().BuildMock().Object);

      // Act
      Assert.Empty(itemService.AllAsync().Result);

      mockItemRepository.Verify(r => r.All(), Times.Once());
    }

    [Fact]
    public void AllAsync_When_ItemsExist_Expect_Returned()
    {
      Item firstItem = new Item { Id = 1, Text = "firstText", Priority = 1 };
      Item secondItem = new Item { Id = 2, Text = "secondText", Priority = 2 };

      mockItemRepository
        .Setup(r => r.All())
        .Returns(new List<Item> { firstItem, secondItem }.AsQueryable().BuildMock().Object);

      IEnumerable<ItemDTO> expected = new List<ItemDTO>
      {
        new ItemDTO { Id = firstItem.Id, Text = firstItem.Text, Priority = firstItem.Priority },
        new ItemDTO { Id = secondItem.Id, Text = secondItem.Text, Priority = secondItem.Priority }
      };

      // Act
      IEnumerable<ItemDTO> actual = itemService.AllAsync().Result;

      actual.ShouldBeEquivalentTo(expected);

      mockItemRepository.Verify(r => r.All(), Times.Once());
    }

    [Fact]
    public void SaveAsync_Expect_Saved()
    {
      int generatedItemId = 10;
      int maxPriority = 1;

      ItemDTO itemToSave = new ItemDTO { Text = "itemText" };

      mockItemRepository
        .Setup(r => r.GetMaxItemPriorityAsync())
        .Returns(Task.FromResult(maxPriority));

      mockItemRepository
        .Setup(r => r.CreateAsync(It.IsAny<Item>()))
        .Callback<Item>(i => i.Id = generatedItemId)
        .Returns(Task.CompletedTask);

      mockItemRepository
        .Setup(m => m.SaveChangesAsync())
        .Returns(Task.CompletedTask);

      ItemDTO expected = new ItemDTO { Id = generatedItemId, Text = itemToSave.Text, Priority = maxPriority + 1 };

      // Act
      ItemDTO actual = itemService.SaveAsync(itemToSave).Result;

      actual.ShouldBeEquivalentTo(expected);

      mockItemRepository.Verify(r => r.GetMaxItemPriorityAsync(), Times.Once());
      mockItemRepository.Verify(r => r.CreateAsync(It.IsAny<Item>()), Times.Once());
      mockItemRepository.Verify(r => r.SaveChangesAsync(), Times.Once());
    }

    [Fact]
    public void UpdatePartiallyAsync_When_PatchCollectionIsNull_Expect_ArgumentException()
    {
      // Act
      Assert.ThrowsAsync<ArgumentException>(() => itemService.UpdatePartiallyAsync(default(int), null));
    }

    [Fact]
    public void UpdatePartiallyAsync_When_ItemDoesNotExist_Expect_EntityNotFoundException()
    {
      int itemId = 1;

      Item notFoundItem = null;

      mockItemRepository
        .Setup(r => r.GetByIdAsync(itemId))
        .Returns(Task.FromResult(notFoundItem));

      // Act
      Assert.ThrowsAsync<EntityNotFoundException>(() => itemService.UpdatePartiallyAsync(itemId, Enumerable.Empty<PatchDTO>().ToList()));

      mockItemRepository.Verify(r => r.GetByIdAsync(itemId), Times.Once());
    }

    [Fact]
    public void UpdatePartiallyAsync_When_ItemExists_Expect_Updated()
    {
      int itemId = 1;

      ICollection<PatchDTO> patches = new List<PatchDTO>()
      {
        new PatchDTO { Name = "Text", Value = "newItemText" }
      };

      Item foundItem = new Item { Id = itemId, Text = "itemText", Priority = 1 };

      mockItemRepository
        .Setup(r => r.GetByIdAsync(itemId))
        .Returns(Task.FromResult(foundItem));

      IDictionary<string, object> expectedDictionary = patches.ToDictionary(p => p.Name, p => p.Value);

      mockItemRepository
        .Setup(r => r.UpdatePartially(foundItem, It.IsAny<IDictionary<string, object>>()))
        .Callback<Item, IDictionary<string, object>>(
          (i, d) =>
          {
            d.ShouldBeEquivalentTo(expectedDictionary);
          }
        )
        .Verifiable();

      mockItemRepository
        .Setup(m => m.SaveChangesAsync())
        .Returns(Task.CompletedTask);

      // Act
      itemService.UpdatePartiallyAsync(itemId, patches).Wait();

      mockItemRepository.Verify(r => r.GetByIdAsync(itemId), Times.Once());
      mockItemRepository.Verify(r => r.UpdatePartially(foundItem, It.IsAny<IDictionary<string, object>>()), Times.Once());

      mockItemRepository.Verify(r => r.SaveChangesAsync(), Times.Once());
    }

    [Fact]
    public void DeleteAsync_When_ItemDoesNotExist_Expect_EntityNotFoundException()
    {
      int itemToDeleteId = 1;

      Item notFoundItem = null;

      mockItemRepository
        .Setup(r => r.GetByIdAsync(itemToDeleteId))
        .Returns(Task.FromResult(notFoundItem));

      // Act
      Assert.ThrowsAsync<EntityNotFoundException>(() => itemService.DeleteAsync(itemToDeleteId));

      mockItemRepository.Verify(r => r.GetByIdAsync(itemToDeleteId), Times.Once());
    }

    [Fact]
    public void DeleteAsync_When_ItemExists_Expect_Deleted()
    {
      int itemToDeleteId = 1;

      Item foundItem = new Item { Id = itemToDeleteId, Text = "itemText", Priority = 1 };

      mockItemRepository
        .Setup(r => r.GetByIdAsync(itemToDeleteId))
        .Returns(Task.FromResult(foundItem));

      mockItemRepository
        .Setup(r => r.Delete(foundItem))
        .Verifiable();

      mockItemRepository
        .Setup(m => m.SaveChangesAsync())
        .Returns(Task.CompletedTask);

      // Act
      itemService.DeleteAsync(itemToDeleteId).Wait();

      mockItemRepository.Verify(r => r.GetByIdAsync(itemToDeleteId), Times.Once());
      mockItemRepository.Verify(r => r.Delete(foundItem), Times.Once());

      mockItemRepository.Verify(m => m.SaveChangesAsync(), Times.Once());
    }
  }
}