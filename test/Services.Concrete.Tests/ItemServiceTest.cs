using Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using Repositories;
using Services.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Services.Tests
{
  public class ItemServiceTest
  {
    private const int userId = 10;

    private readonly Mock<IItemRepository> mockItemRepository;
    private readonly Mock<ITransactionManager> mockTransactionManager;

    private readonly IItemService itemService;

    public ItemServiceTest()
    {
      this.mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
      this.mockTransactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);

      this.itemService = new ItemService(mockItemRepository.Object, mockTransactionManager.Object);
    }

    public class GetAllAsync : ItemServiceTest
    {
      [Fact]
      public async Task When_ItemsDoNotExist_Expect_EmptyList()
      {
        mockItemRepository
          .Setup(r => r.All(userId))
          .Returns(Enumerable.Empty<Item>().AsQueryable().BuildMock().Object);

        // Act
        Assert.Empty(await itemService.GetAllAsync(userId));

        mockItemRepository.Verify(r => r.All(userId), Times.Once());
      }

      [Fact]
      public async Task When_ItemsExist_Expect_Returned()
      {
        Item firstItem = new Item
        {
          Id = 1,
          UserId = userId,
          Text = "firstText",
          Priority = 1,
          Status = ItemStatus.Todo
        };

        Item secondItem = new Item
        {
          Id = 2,
          UserId = userId,
          Text = "secondText",
          Priority = 2,
          Status = ItemStatus.Done
        };

        mockItemRepository
          .Setup(r => r.All(userId))
          .Returns(new List<Item> { firstItem, secondItem }.AsQueryable().BuildMock().Object);

        IEnumerable<ItemDTO> expected = new List<ItemDTO>
        {
          new ItemDTO {
            Id = firstItem.Id,
            UserId = firstItem.UserId,
            IsDone = firstItem.Status == ItemStatus.Done,
            Text = firstItem.Text,
            Priority = firstItem.Priority
          },

          new ItemDTO {
            Id = secondItem.Id,
            UserId = secondItem.UserId,
            IsDone = secondItem.Status == ItemStatus.Done,
            Text = secondItem.Text,
            Priority = secondItem.Priority
          }
        };

        // Act
        IEnumerable<ItemDTO> actual = await itemService.GetAllAsync(userId);

        actual.ShouldBeEquivalentTo(expected);

        mockItemRepository.Verify(r => r.All(userId), Times.Once());
      }
    }

    public class SaveAsync : ItemServiceTest
    {
      [Fact]
      public async Task Expect_Saved()
      {
        int generatedItemId = 10;
        int maxPriority = 1;

        ItemDTO itemToSave = new ItemDTO { UserId = userId, Text = "itemText" };

        mockItemRepository
          .Setup(r => r.GetMaxItemPriorityAsync(userId))
          .ReturnsAsync(maxPriority);

        mockItemRepository
          .Setup(r => r.CreateAsync(It.IsAny<Item>()))
          .Callback<Item>(i => i.Id = generatedItemId)
          .Returns(Task.CompletedTask);

        mockTransactionManager
          .Setup(m => m.SaveChangesAsync())
          .Returns(Task.CompletedTask);

        ItemDTO expected = new ItemDTO
        {
          Id = generatedItemId,
          UserId = itemToSave.UserId,
          IsDone = false,
          Text = itemToSave.Text,
          Priority = maxPriority + 1
        };

        // Act
        ItemDTO actual = await itemService.SaveAsync(itemToSave);

        actual.ShouldBeEquivalentTo(expected);

        mockItemRepository.Verify(r => r.GetMaxItemPriorityAsync(userId), Times.Once());
        mockItemRepository.Verify(r => r.CreateAsync(It.IsAny<Item>()), Times.Once());
        mockTransactionManager.Verify(m => m.SaveChangesAsync(), Times.Once());
      }
    }

    public class UpdateAsync : ItemServiceTest
    {
      [Fact]
      public async Task When_ItemDoesNotExist_Expect_EntityNotFoundException()
      {
        int itemToUpdateId = 1;

        Item notFoundItem = null;

        mockItemRepository
          .Setup(r => r.GetByIdAndUserIdAsync(itemToUpdateId, userId))
          .ReturnsAsync(notFoundItem);

        ItemDTO itemToUpdate = new ItemDTO { Id = itemToUpdateId };

        // Act
        await Assert.ThrowsAsync<EntityNotFoundException>(() => itemService.UpdateAsync(userId, itemToUpdate));

        mockItemRepository.Verify(r => r.GetByIdAndUserIdAsync(itemToUpdateId, userId), Times.Once());
      }

      [Fact]
      public async Task When_ItemExists_Expect_Updated()
      {
        int itemToUpdateId = 1;

        Item foundItem = new Item
        {
          Id = itemToUpdateId,
          UserId = userId,
          Status = ItemStatus.Todo,
          Text = "itemText",
          Priority = 5
        };

        mockItemRepository
          .Setup(r => r.GetByIdAndUserIdAsync(itemToUpdateId, userId))
          .ReturnsAsync(foundItem);

        mockTransactionManager
          .Setup(m => m.SaveChangesAsync())
          .Returns(Task.CompletedTask);

        ItemDTO itemToUpdate = new ItemDTO
        {
          Id = itemToUpdateId,
          IsDone = true,
          Text = "newItemText",
          Priority = 10
        };

        // Act
        await itemService.UpdateAsync(userId, itemToUpdate);

        Assert.Equal(ItemStatus.Done, foundItem.Status);
        Assert.Equal(itemToUpdate.Text, foundItem.Text);
        Assert.Equal(itemToUpdate.Priority, foundItem.Priority);

        mockItemRepository.Verify(r => r.GetByIdAndUserIdAsync(itemToUpdateId, userId), Times.Once());

        mockTransactionManager.Verify(m => m.SaveChangesAsync(), Times.Once());
      }
    }

    public class DeleteAsync : ItemServiceTest
    {
      [Fact]
      public async Task When_ItemDoesNotExist_Expect_EntityNotFoundException()
      {
        int itemToDeleteId = 1;

        Item notFoundItem = null;

        mockItemRepository
          .Setup(r => r.GetByIdAndUserIdAsync(itemToDeleteId, userId))
          .ReturnsAsync(notFoundItem);

        // Act
        await Assert.ThrowsAsync<EntityNotFoundException>(() => itemService.DeleteAsync(itemToDeleteId, userId));

        mockItemRepository.Verify(r => r.GetByIdAndUserIdAsync(itemToDeleteId, userId), Times.Once());
      }

      [Fact]
      public async Task When_ItemExists_Expect_Deleted()
      {
        int itemToDeleteId = 1;

        Item foundItem = new Item { Id = itemToDeleteId, Text = "itemText", Priority = 1 };

        mockItemRepository
          .Setup(r => r.GetByIdAndUserIdAsync(itemToDeleteId, userId))
          .ReturnsAsync(foundItem);

        mockItemRepository
          .Setup(r => r.Delete(foundItem))
          .Verifiable();

        mockTransactionManager
          .Setup(m => m.SaveChangesAsync())
          .Returns(Task.CompletedTask);

        // Act
        await itemService.DeleteAsync(itemToDeleteId, userId);

        mockItemRepository.Verify(r => r.GetByIdAndUserIdAsync(itemToDeleteId, userId), Times.Once());
        mockItemRepository.Verify(r => r.Delete(foundItem), Times.Once());

        mockTransactionManager.Verify(m => m.SaveChangesAsync(), Times.Once());
      }
    }
  }
}
