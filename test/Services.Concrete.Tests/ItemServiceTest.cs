using Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using Repositories;
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
      mockItemRepository.Setup(r => r.All()).Returns(new List<Item>().AsQueryable().BuildMock().Object);

      // Act
      Assert.Empty(itemService.AllAsync().Result);

      mockItemRepository.Verify(r => r.All(), Times.Once());
    }

    [Fact]
    public void AllAsync_When_ItemsExist_Expect_Returned()
    {
      Item firstItem = new Item { Id = 1, Text = "firstText" };
      Item secondItem = new Item { Id = 2, Text = "secondText" };

      mockItemRepository
        .Setup(r => r.All())
        .Returns(new List<Item> { firstItem, secondItem }.AsQueryable().BuildMock().Object);

      IEnumerable<ItemDTO> expected = new List<ItemDTO>
      {
        new ItemDTO { Id = firstItem.Id, Text = firstItem.Text },
        new ItemDTO { Id = secondItem.Id, Text = secondItem.Text }
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

      ItemDTO itemToSave = new ItemDTO { Text = "itemText" };

      mockItemRepository
        .Setup(r => r.CreateAsync(It.IsAny<Item>()))
        .Callback<Item>(i => i.Id = generatedItemId)
        .Returns(Task.CompletedTask);

      mockItemRepository.Setup(m => m.SaveChangesAsync()).Returns(Task.CompletedTask);

      ItemDTO expected = new ItemDTO { Id = generatedItemId, Text = itemToSave.Text };

      // Act
      ItemDTO actual = itemService.SaveAsync(itemToSave).Result;

      actual.ShouldBeEquivalentTo(expected);

      mockItemRepository.Verify(r => r.CreateAsync(It.IsAny<Item>()), Times.Once());
      mockItemRepository.Verify((m => m.SaveChangesAsync()), Times.Once());
    }

    [Fact]
    public void UpdateAsync_When_ItemDoesNotExist_Expect_ArgumentException()
    {
      ItemDTO itemToUpdate = new ItemDTO { Id = 1, Text = "itemText" };
      Item notFoundItem = null;

      mockItemRepository.Setup(r => r.GetByIdAsync(itemToUpdate.Id)).Returns(Task.FromResult(notFoundItem));

      // Act
      Assert.ThrowsAsync<ArgumentException>(() => itemService.UpdateAsync(itemToUpdate));

      mockItemRepository.Verify(r => r.GetByIdAsync(itemToUpdate.Id), Times.Once());
    }

    [Fact]
    public void UpdateAsync_When_ItemExists_Expect_Nothing()
    {
      ItemDTO itemToUpdate = new ItemDTO { Id = 1, Text = "newItemText" };
      Item foundItem = new Item { Id = itemToUpdate.Id, Text = "itemText" };

      mockItemRepository.Setup(r => r.GetByIdAsync(itemToUpdate.Id)).Returns(Task.FromResult(foundItem));

      mockItemRepository
        .Setup(r => r.Update(foundItem))
        .Callback<Item>(item => item.ShouldBeEquivalentTo(itemToUpdate))
        .Verifiable();

      mockItemRepository.Setup(m => m.SaveChangesAsync()).Returns(Task.CompletedTask);

      // Act
      itemService.UpdateAsync(itemToUpdate).Wait();

      mockItemRepository.Verify(r => r.GetByIdAsync(itemToUpdate.Id), Times.Once());
      mockItemRepository.Verify(r => r.Update(foundItem), Times.Once());

      mockItemRepository.Verify(m => m.SaveChangesAsync(), Times.Once());
    }

    [Fact]
    public void DeleteAsync_When_ItemDoesNotExist_Expect_ArgumentException()
    {
      int itemToDeleteId = 1;

      Item notFoundItem = null;

      mockItemRepository.Setup(r => r.GetByIdAsync(itemToDeleteId)).Returns(Task.FromResult(notFoundItem));

      // Act
      Assert.ThrowsAsync<ArgumentException>(() => itemService.DeleteAsync(itemToDeleteId));

      mockItemRepository.Verify(r => r.GetByIdAsync(itemToDeleteId), Times.Once());
    }

    [Fact]
    public void DeleteAsync_When_ItemExists_Expect_Nothing()
    {
      int itemToDeleteId = 1;

      Item foundItem = new Item { Id = itemToDeleteId, Text = "itemText" };

      mockItemRepository.Setup(r => r.GetByIdAsync(itemToDeleteId)).Returns(Task.FromResult(foundItem));
      mockItemRepository.Setup(r => r.Delete(foundItem)).Verifiable();

      mockItemRepository.Setup(m => m.SaveChangesAsync()).Returns(Task.CompletedTask);

      // Act
      itemService.DeleteAsync(itemToDeleteId).Wait();

      mockItemRepository.Verify(r => r.GetByIdAsync(itemToDeleteId), Times.Once());
      mockItemRepository.Verify(r => r.Delete(foundItem), Times.Once());

      mockItemRepository.Verify(m => m.SaveChangesAsync(), Times.Once());
    }
  }
}