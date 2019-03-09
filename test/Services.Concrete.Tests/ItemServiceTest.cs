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
    private const int userId = 10;

    private readonly Mock<IItemRepository> mockItemRepository;
    private readonly Mock<ITransactionManager> mockTransactionManager;

    private readonly ItemService itemService;

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
          .Returns(new List<Item>().AsQueryable().BuildMock().Object);

        // Act
        Assert.Empty(await itemService.GetAllAsync(userId));

        mockItemRepository.Verify(r => r.All(userId), Times.Once());
      }

      [Fact]
      public async Task When_ItemsExist_Expect_Returned()
      {
        Item firstItem = new Item { Id = 1, Text = "firstText", Priority = 1, Status = ItemStatus.Todo };
        Item secondItem = new Item { Id = 2, Text = "secondText", Priority = 2, Status = ItemStatus.Done };

        mockItemRepository
          .Setup(r => r.All(userId))
          .Returns(new List<Item> { firstItem, secondItem }.AsQueryable().BuildMock().Object);

        IEnumerable<ItemDTO> expected = new List<ItemDTO>
        {
          new ItemDTO { Id = firstItem.Id, Text = firstItem.Text, Priority = firstItem.Priority, StatusId = (int)firstItem.Status },
          new ItemDTO { Id = secondItem.Id, Text = secondItem.Text, Priority = secondItem.Priority, StatusId = (int)secondItem.Status }
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
          Text = itemToSave.Text,
          Priority = maxPriority + 1,
          StatusId = (int)ItemStatus.Todo
        };

        // Act
        ItemDTO actual = await itemService.SaveAsync(itemToSave);

        actual.ShouldBeEquivalentTo(expected);

        mockItemRepository.Verify(r => r.GetMaxItemPriorityAsync(userId), Times.Once());
        mockItemRepository.Verify(r => r.CreateAsync(It.IsAny<Item>()), Times.Once());
        mockTransactionManager.Verify(m => m.SaveChangesAsync(), Times.Once());
      }
    }

    public class UpdatePartiallyAsync : ItemServiceTest
    {
      [Fact]
      public async Task When_PatchCollectionIsNull_Expect_ArgumentException()
      {
        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => itemService.UpdatePartiallyAsync(default(int), default(int), null));
      }

      [Fact]
      public async Task When_ItemDoesNotExist_Expect_EntityNotFoundException()
      {
        int itemId = 1;

        Item notFoundItem = null;

        mockItemRepository
          .Setup(r => r.GetByIdAndUserIdAsync(itemId, userId))
          .ReturnsAsync(notFoundItem);

        // Act
        await Assert.ThrowsAsync<EntityNotFoundException>(() => itemService.UpdatePartiallyAsync(itemId, userId, Enumerable.Empty<PatchDTO>().ToList()));

        mockItemRepository.Verify(r => r.GetByIdAndUserIdAsync(itemId, userId), Times.Once());
      }

      [Fact]
      public async Task When_ItemExists_Expect_Updated()
      {
        int itemId = 1;

        PatchDTO statusPatchDTO = new PatchDTO { Name = "StatusId", Value = (long)(int)ItemStatus.Done };

        ICollection<PatchDTO> patches = new List<PatchDTO>()
        {
          new PatchDTO { Name = "Text", Value = "newItemText" },
          new PatchDTO { Name = "Priority", Value = 2L },
          statusPatchDTO
        };

        Item foundItem = new Item { Id = itemId, UserId = userId, Text = "itemText", Priority = 1, Status = ItemStatus.Todo };

        mockItemRepository
          .Setup(r => r.GetByIdAndUserIdAsync(itemId, userId))
          .ReturnsAsync(foundItem);

        IDictionary<string, object> expectedDictionary = patches.ToDictionary(p => p.Name, p => p.Value);

        expectedDictionary.Remove("StatusId");
        expectedDictionary.Add("Status", statusPatchDTO.Value);

        mockItemRepository
          .Setup(r => r.UpdatePartially(foundItem, It.IsAny<IDictionary<string, object>>()))
          .Callback<Item, IDictionary<string, object>>(
            (i, d) =>
            {
              d.ShouldBeEquivalentTo(expectedDictionary);
            }
          )
          .Verifiable();

        mockTransactionManager
          .Setup(m => m.SaveChangesAsync())
          .Returns(Task.CompletedTask);

        // Act
        await itemService.UpdatePartiallyAsync(itemId, userId, patches);

        mockItemRepository.Verify(r => r.GetByIdAndUserIdAsync(itemId, userId), Times.Once());
        mockItemRepository.Verify(r => r.UpdatePartially(foundItem, It.IsAny<IDictionary<string, object>>()), Times.Once());

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
