using Delegates;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Controllers.Services.Tests
{
  public class CachedItemServiceTest : IDisposable
  {
    private const int userId = 1;

    private readonly Mock<ItemServiceResolver> mockItemServiceResolver;
    private readonly Mock<IItemService> mockItemService;
    private readonly IMemoryCache mockMemoryCache;

    private readonly IItemService cachedItemService;

    public CachedItemServiceTest()
    {
      this.mockItemServiceResolver = new Mock<ItemServiceResolver>(MockBehavior.Strict);
      this.mockItemService = new Mock<IItemService>(MockBehavior.Strict);
      this.mockMemoryCache = new MemoryCache(new MemoryCacheOptions());

      this.mockItemServiceResolver
        .Setup(r => r("main"))
        .Returns(mockItemService.Object);

      this.cachedItemService = new CachedItemService(mockItemServiceResolver.Object, mockMemoryCache);
    }

    public class GetAllAsync : CachedItemServiceTest
    {
      [Fact]
      public async Task When_ItemsDoNotExistInCache_Expect_CachedAndReturned()
      {
        IEnumerable<ItemDTO> expectedUserItems = new[] {
          new ItemDTO { Id = 1, UserId = userId },
          new ItemDTO { Id = 2, UserId = userId }
        };

        mockItemService
          .Setup(s => s.GetAllAsync(userId))
          .ReturnsAsync(expectedUserItems);

        Assert.Null(mockMemoryCache.Get(userId));

        // Act
        IEnumerable<ItemDTO> actualUserItems = await cachedItemService.GetAllAsync(userId);
        IEnumerable<ItemDTO> userItemsFromCache = (IEnumerable<ItemDTO>)mockMemoryCache.Get(userId);

        actualUserItems.ShouldBeEquivalentTo(expectedUserItems);
        userItemsFromCache.ShouldBeEquivalentTo(expectedUserItems);

        mockItemService.Verify(s => s.GetAllAsync(userId), Times.Once());
      }

      [Fact]
      public async Task When_ItemsExistInCache_Expect_ReturnedFromCache()
      {
        IEnumerable<ItemDTO> expectedUserItems = new[] {
          new ItemDTO { Id = 1, UserId = userId },
          new ItemDTO { Id = 2, UserId = userId }
        };

        mockMemoryCache.Set(userId, expectedUserItems);

        // Act
        IEnumerable<ItemDTO> actualUserItems = await cachedItemService.GetAllAsync(userId);

        actualUserItems.ShouldBeEquivalentTo(expectedUserItems);

        mockItemService.Verify(s => s.GetAllAsync(userId), Times.Never());
      }
    }

    public class SaveAsync : CachedItemServiceTest
    {
      [Fact]
      public async Task When_ItemIsSaved_Expect_CacheIsDropped()
      {
        ItemDTO itemToSave = new ItemDTO { UserId = userId, Text = "test-text" };
        ItemDTO expectedSavedItem = new ItemDTO { Id = 1, UserId = userId, Text = "test-text", Priority = 1, IsDone = false };

        mockItemService
          .Setup(s => s.SaveAsync(itemToSave))
          .ReturnsAsync(expectedSavedItem);

        mockMemoryCache.Set(userId, new ItemDTO[] { });

        Assert.NotNull(mockMemoryCache.Get(userId));

        // Act
        ItemDTO actualSavedItem = await cachedItemService.SaveAsync(itemToSave);

        actualSavedItem.ShouldBeEquivalentTo(expectedSavedItem);
        Assert.Null(mockMemoryCache.Get(userId));

        mockItemService.Verify(s => s.SaveAsync(itemToSave), Times.Once());
      }
    }

    public class UpdateAsync : CachedItemServiceTest
    {
      [Fact]
      public async Task When_ItemIsUpdated_Expect_CacheIsDropped()
      {
        ItemDTO itemToUpdate = new ItemDTO { UserId = userId };

        mockItemService
          .Setup(s => s.UpdateAsync(userId, itemToUpdate))
          .Returns(Task.CompletedTask);

        mockMemoryCache.Set(userId, new ItemDTO[] { });

        Assert.NotNull(mockMemoryCache.Get(userId));

        // Act
        await cachedItemService.UpdateAsync(userId, itemToUpdate);

        Assert.Null(mockMemoryCache.Get(userId));

        mockItemService.Verify(s => s.UpdateAsync(userId, itemToUpdate), Times.Once());
      }
    }

    public class DeleteAsync : CachedItemServiceTest
    {
      [Fact]
      public async Task When_ItemIsDeleted_Expect_CacheIsDropped()
      {
        int itemToDeleteId = 2;

        mockItemService
          .Setup(s => s.DeleteAsync(itemToDeleteId, userId))
          .Returns(Task.CompletedTask);

        mockMemoryCache.Set(userId, new ItemDTO[] { });

        Assert.NotNull(mockMemoryCache.Get(userId));

        // Act
        await cachedItemService.DeleteAsync(itemToDeleteId, userId);

        Assert.Null(mockMemoryCache.Get(userId));

        mockItemService.Verify(s => s.DeleteAsync(itemToDeleteId, userId), Times.Once());
      }
    }

    public void Dispose()
    {
      mockItemServiceResolver.Verify(r => r("main"), Times.Once());
    }
  }
}
