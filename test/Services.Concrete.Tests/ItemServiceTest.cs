using Entities;
using FluentAssertions;
using Moq;
using Repositories;
using System.Collections.Generic;
using Xunit;

namespace Services.Tests
{
  public class ItemServiceTest
  {
    private readonly Mock<IItemRepository> mockItemRepository;
    private readonly Mock<IDbTransactionManager> mockDbTransactionManager;

    private readonly ItemService itemService;

    public ItemServiceTest()
    {
      this.mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
      this.mockDbTransactionManager = new Mock<IDbTransactionManager>(MockBehavior.Strict);

      this.itemService = new ItemService(mockItemRepository.Object, mockDbTransactionManager.Object);
    }

    [Fact]
    public void All_When_ItemsDoNotExist_Expect_EmptyList()
    {
      mockItemRepository.Setup(r => r.All()).Returns(new List<Item>());

      Assert.Empty(itemService.All());

      mockItemRepository.Verify(r => r.All(), Times.Once());
    }

    [Fact]
    public void All_When_ItemsExist_Expect_Returned()
    {
      Item firstItem = new Item { Id = 1, Text = "firstText" };
      Item secondItem = new Item { Id = 2, Text = "secondText" };

      mockItemRepository.Setup(r => r.All()).Returns(new List<Item> { firstItem, secondItem });

      IEnumerable<ItemDTO> expected = new List<ItemDTO>
      {
        new ItemDTO { Id = firstItem.Id, Text = firstItem.Text },
        new ItemDTO { Id = secondItem.Id, Text = secondItem.Text }
      };

      IEnumerable<ItemDTO> actual = itemService.All();

      actual.ShouldBeEquivalentTo(expected);

      mockItemRepository.Verify(r => r.All(), Times.Once());
    }

    [Fact]
    public void Save_Expect_Saved()
    {
      int generatedItemId = 10;

      ItemDTO itemToSave = new ItemDTO { Text = "itemText" };

      mockItemRepository
        .Setup(r => r.Create(It.IsAny<Item>()))
        .Callback<Item>(i => i.Id = generatedItemId)
        .Verifiable();

      mockDbTransactionManager.Setup(m => m.SaveChanges()).Verifiable();

      ItemDTO expected = new ItemDTO { Id = generatedItemId, Text = itemToSave.Text };
      ItemDTO actual = itemService.Save(itemToSave);

      actual.ShouldBeEquivalentTo(expected);

      mockItemRepository.Verify(r => r.Create(It.IsAny<Item>()), Times.Once());
      mockDbTransactionManager.Verify((m => m.SaveChanges()), Times.Once());
    }

    [Fact]
    public void Update_When_ItemDoesNotExist_Expect_NotSuccessedOperation()
    {
      ItemDTO itemToUpdate = new ItemDTO { Id = 1, Text = "itemText" };
      Item notFoundItem = null;

      mockItemRepository.Setup(r => r.GetById(itemToUpdate.Id)).Returns(notFoundItem);

      Assert.False(itemService.Update(itemToUpdate).Success);

      mockItemRepository.Verify(r => r.GetById(itemToUpdate.Id), Times.Once());
    }

    [Fact]
    public void Update_When_ItemExists_Expect_SuccessedOperation()
    {
      ItemDTO itemToUpdate = new ItemDTO { Id = 1, Text = "newItemText" };
      Item foundItem = new Item { Id = itemToUpdate.Id, Text = "itemText" };

      mockItemRepository.Setup(r => r.GetById(itemToUpdate.Id)).Returns(foundItem);

      mockItemRepository
        .Setup(r => r.Update(foundItem))
        .Callback<Item>(item => item.ShouldBeEquivalentTo(itemToUpdate))
        .Verifiable();

      mockDbTransactionManager.Setup(m => m.SaveChanges()).Verifiable();

      Assert.True(itemService.Update(itemToUpdate).Success);

      mockItemRepository.Verify(r => r.GetById(itemToUpdate.Id), Times.Once());
      mockItemRepository.Verify(r => r.Update(foundItem), Times.Once());

      mockDbTransactionManager.Verify(m => m.SaveChanges(), Times.Once());
    }

    [Fact]
    public void Delete_When_ItemDoesNotExist_Expect_NotSuccessedOperation()
    {
      int itemToDeleteId = 1;

      Item notFoundItem = null;

      mockItemRepository.Setup(r => r.GetById(itemToDeleteId)).Returns(notFoundItem);

      Assert.False(itemService.Delete(itemToDeleteId).Success);

      mockItemRepository.Verify(r => r.GetById(itemToDeleteId), Times.Once());
    }

    [Fact]
    public void Delete_When_ItemExists_Expect_SuccessedOperation()
    {
      int itemToDeleteId = 1;

      Item foundItem = new Item { Id = itemToDeleteId, Text = "itemText" };

      mockItemRepository.Setup(r => r.GetById(itemToDeleteId)).Returns(foundItem);
      mockItemRepository.Setup(r => r.Delete(itemToDeleteId)).Verifiable();

      mockDbTransactionManager.Setup(m => m.SaveChanges()).Verifiable();

      Assert.True(itemService.Delete(itemToDeleteId).Success);

      mockItemRepository.Verify(r => r.GetById(itemToDeleteId), Times.Once());
      mockItemRepository.Verify(r => r.Delete(itemToDeleteId), Times.Once());

      mockDbTransactionManager.Verify(m => m.SaveChanges(), Times.Once());
    }
  }
}