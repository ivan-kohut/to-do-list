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
    private const int identityId = 10;

    private readonly Mock<IItemRepository> mockItemRepository;
    private readonly Mock<IUserRepository> mockUserRepository;
    private readonly Mock<ITransactionManager> mockTransactionManager;

    private readonly IItemService itemService;

    public ItemServiceTest()
    {
      this.mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
      this.mockUserRepository = new Mock<IUserRepository>(MockBehavior.Strict);
      this.mockTransactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);

      this.itemService = new ItemService(mockItemRepository.Object, mockUserRepository.Object, mockTransactionManager.Object);
    }

    public class GetAllAsync : ItemServiceTest
    {
      [Fact]
      public async Task When_ItemsDoNotExist_Expect_EmptyList()
      {
        mockItemRepository
          .Setup(r => r.All(identityId))
          .Returns(Enumerable.Empty<Item>().AsQueryable().BuildMock().Object);

        // Act
        Assert.Empty(await itemService.GetAllAsync(identityId));

        mockItemRepository.Verify(r => r.All(identityId), Times.Once());
      }

      [Fact]
      public async Task When_ItemsExist_Expect_Returned()
      {
        Item firstItem = new Item
        {
          Id = 1,
          UserId = 25,
          Text = "firstText",
          Priority = 1,
          Status = ItemStatus.Todo
        };

        Item secondItem = new Item
        {
          Id = 2,
          UserId = 25,
          Text = "secondText",
          Priority = 2,
          Status = ItemStatus.Done
        };

        mockItemRepository
          .Setup(r => r.All(identityId))
          .Returns(new List<Item> { firstItem, secondItem }.AsQueryable().BuildMock().Object);

        IEnumerable<ItemDTO> expected = new List<ItemDTO>
        {
          new ItemDTO {
            Id = firstItem.Id,
            IsDone = firstItem.Status == ItemStatus.Done,
            Text = firstItem.Text,
            Priority = firstItem.Priority
          },

          new ItemDTO {
            Id = secondItem.Id,
            IsDone = secondItem.Status == ItemStatus.Done,
            Text = secondItem.Text,
            Priority = secondItem.Priority
          }
        };

        // Act
        IEnumerable<ItemDTO> actual = await itemService.GetAllAsync(identityId);

        actual.ShouldBeEquivalentTo(expected);

        mockItemRepository.Verify(r => r.All(identityId), Times.Once());
      }
    }

    public class SaveAsync : ItemServiceTest
    {
      [Fact]
      public async Task When_UserDoesNotExist_Expect_EntityNotFoundException()
      {
        User? user = null;

        mockUserRepository
          .Setup(r => r.GetUserAsync(identityId))
          .ReturnsAsync(user);

        // Act
        await Assert.ThrowsAsync<EntityNotFoundException>(() => itemService.SaveAsync(identityId, new ItemDTO()));

        mockUserRepository.Verify(r => r.GetUserAsync(identityId), Times.Once);
      }

      [Fact]
      public async Task Expect_Saved()
      {
        int generatedItemId = 10;
        int maxPriority = 1;

        User user = new User
        {
          Id = 25,
          IdentityId = identityId
        };

        mockUserRepository
          .Setup(r => r.GetUserAsync(identityId))
          .ReturnsAsync(user);

        ItemDTO itemToSave = new ItemDTO { Text = "itemText" };

        mockItemRepository
          .Setup(r => r.GetMaxItemPriorityAsync(identityId))
          .ReturnsAsync(maxPriority);

        mockItemRepository
          .Setup(r => r.Create(It.IsAny<Item>()))
          .Callback<Item>(i =>
          {
            Assert.Equal(user.Id, i.UserId);

            i.Id = generatedItemId;
          });

        mockTransactionManager
          .Setup(m => m.SaveChangesAsync())
          .Returns(Task.CompletedTask);

        ItemDTO expected = new ItemDTO
        {
          Id = generatedItemId,
          IsDone = false,
          Text = itemToSave.Text,
          Priority = maxPriority + 1
        };

        // Act
        ItemDTO actual = await itemService.SaveAsync(identityId, itemToSave);

        actual.ShouldBeEquivalentTo(expected);

        mockUserRepository.Verify(r => r.GetUserAsync(identityId), Times.Once());
        mockItemRepository.Verify(r => r.GetMaxItemPriorityAsync(identityId), Times.Once());
        mockItemRepository.Verify(r => r.Create(It.IsAny<Item>()), Times.Once());
        mockTransactionManager.Verify(m => m.SaveChangesAsync(), Times.Once());
      }
    }

    public class UpdateAsync : ItemServiceTest
    {
      [Fact]
      public async Task When_ItemDoesNotExist_Expect_EntityNotFoundException()
      {
        int itemToUpdateId = 1;

        Item? notFoundItem = null;

        mockItemRepository
          .Setup(r => r.GetByIdAndIdentityIdAsync(itemToUpdateId, identityId))
          .ReturnsAsync(notFoundItem);

        ItemDTO itemToUpdate = new ItemDTO { Id = itemToUpdateId };

        // Act
        await Assert.ThrowsAsync<EntityNotFoundException>(() => itemService.UpdateAsync(identityId, itemToUpdate));

        mockItemRepository.Verify(r => r.GetByIdAndIdentityIdAsync(itemToUpdateId, identityId), Times.Once());
      }

      [Fact]
      public async Task When_ItemExists_Expect_Updated()
      {
        int itemToUpdateId = 1;

        Item foundItem = new Item
        {
          Id = itemToUpdateId,
          Status = ItemStatus.Todo,
          Text = "itemText",
          Priority = 5
        };

        mockItemRepository
          .Setup(r => r.GetByIdAndIdentityIdAsync(itemToUpdateId, identityId))
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
        await itemService.UpdateAsync(identityId, itemToUpdate);

        Assert.Equal(ItemStatus.Done, foundItem.Status);
        Assert.Equal(itemToUpdate.Text, foundItem.Text);
        Assert.Equal(itemToUpdate.Priority, foundItem.Priority);

        mockItemRepository.Verify(r => r.GetByIdAndIdentityIdAsync(itemToUpdateId, identityId), Times.Once());

        mockTransactionManager.Verify(m => m.SaveChangesAsync(), Times.Once());
      }
    }

    public class DeleteAsync : ItemServiceTest
    {
      [Fact]
      public async Task When_ItemDoesNotExist_Expect_EntityNotFoundException()
      {
        int itemToDeleteId = 1;

        Item? notFoundItem = null;

        mockItemRepository
          .Setup(r => r.GetByIdAndIdentityIdAsync(itemToDeleteId, identityId))
          .ReturnsAsync(notFoundItem);

        // Act
        await Assert.ThrowsAsync<EntityNotFoundException>(() => itemService.DeleteAsync(itemToDeleteId, identityId));

        mockItemRepository.Verify(r => r.GetByIdAndIdentityIdAsync(itemToDeleteId, identityId), Times.Once());
      }

      [Fact]
      public async Task When_ItemExists_Expect_Deleted()
      {
        int itemToDeleteId = 1;

        Item foundItem = new Item { Id = itemToDeleteId, Text = "itemText", Priority = 1 };

        mockItemRepository
          .Setup(r => r.GetByIdAndIdentityIdAsync(itemToDeleteId, identityId))
          .ReturnsAsync(foundItem);

        mockItemRepository
          .Setup(r => r.Delete(foundItem))
          .Verifiable();

        mockTransactionManager
          .Setup(m => m.SaveChangesAsync())
          .Returns(Task.CompletedTask);

        // Act
        await itemService.DeleteAsync(itemToDeleteId, identityId);

        mockItemRepository.Verify(r => r.GetByIdAndIdentityIdAsync(itemToDeleteId, identityId), Times.Once());
        mockItemRepository.Verify(r => r.Delete(foundItem), Times.Once());

        mockTransactionManager.Verify(m => m.SaveChangesAsync(), Times.Once());
      }
    }
  }
}
