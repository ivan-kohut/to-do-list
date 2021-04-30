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
    private const int identityId = 1;

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
          new ItemDTO { Id = 1 },
          new ItemDTO { Id = 2 }
        };

        mockItemService
          .Setup(s => s.GetAllAsync(identityId))
          .ReturnsAsync(expectedUserItems);

        Assert.Null(mockMemoryCache.Get(identityId));

        // Act
        IEnumerable<ItemDTO> actualUserItems = await cachedItemService.GetAllAsync(identityId);
        IEnumerable<ItemDTO> userItemsFromCache = (IEnumerable<ItemDTO>)mockMemoryCache.Get(identityId);

        actualUserItems.ShouldBeEquivalentTo(expectedUserItems);
        userItemsFromCache.ShouldBeEquivalentTo(expectedUserItems);

        mockItemService.Verify(s => s.GetAllAsync(identityId), Times.Once());
      }

      [Fact]
      public async Task When_ItemsExistInCache_Expect_ReturnedFromCache()
      {
        IEnumerable<ItemDTO> expectedUserItems = new[] {
          new ItemDTO { Id = 1 },
          new ItemDTO { Id = 2 }
        };

        mockMemoryCache.Set(identityId, expectedUserItems);

        // Act
        IEnumerable<ItemDTO> actualUserItems = await cachedItemService.GetAllAsync(identityId);

        actualUserItems.ShouldBeEquivalentTo(expectedUserItems);

        mockItemService.Verify(s => s.GetAllAsync(identityId), Times.Never());
      }
    }

    public class SaveAsync : CachedItemServiceTest
    {
      [Fact]
      public async Task When_ItemIsSaved_Expect_CacheIsDropped()
      {
        ItemDTO itemToSave = new() { Text = "test-text" };
        ItemDTO expectedSavedItem = new() { Id = 1, Text = "test-text", Priority = 1, IsDone = false };

        mockItemService
          .Setup(s => s.SaveAsync(identityId, itemToSave))
          .ReturnsAsync(expectedSavedItem);

        mockMemoryCache.Set(identityId, Array.Empty<ItemDTO>());

        Assert.NotNull(mockMemoryCache.Get(identityId));

        // Act
        ItemDTO actualSavedItem = await cachedItemService.SaveAsync(identityId, itemToSave);

        actualSavedItem.ShouldBeEquivalentTo(expectedSavedItem);
        Assert.Null(mockMemoryCache.Get(identityId));

        mockItemService.Verify(s => s.SaveAsync(identityId, itemToSave), Times.Once());
      }
    }

    public class UpdateAsync : CachedItemServiceTest
    {
      [Fact]
      public async Task When_ItemIsUpdated_Expect_CacheIsDropped()
      {
        ItemDTO itemToUpdate = new() { Id = 25 };

        mockItemService
          .Setup(s => s.UpdateAsync(identityId, itemToUpdate))
          .Returns(Task.CompletedTask);

        mockMemoryCache.Set(identityId, Array.Empty<ItemDTO>());

        Assert.NotNull(mockMemoryCache.Get(identityId));

        // Act
        await cachedItemService.UpdateAsync(identityId, itemToUpdate);

        Assert.Null(mockMemoryCache.Get(identityId));

        mockItemService.Verify(s => s.UpdateAsync(identityId, itemToUpdate), Times.Once());
      }
    }

    public class DeleteAsync : CachedItemServiceTest
    {
      [Fact]
      public async Task When_ItemIsDeleted_Expect_CacheIsDropped()
      {
        int itemToDeleteId = 2;

        mockItemService
          .Setup(s => s.DeleteAsync(itemToDeleteId, identityId))
          .Returns(Task.CompletedTask);

        mockMemoryCache.Set(identityId, Array.Empty<ItemDTO>());

        Assert.NotNull(mockMemoryCache.Get(identityId));

        // Act
        await cachedItemService.DeleteAsync(itemToDeleteId, identityId);

        Assert.Null(mockMemoryCache.Get(identityId));

        mockItemService.Verify(s => s.DeleteAsync(itemToDeleteId, identityId), Times.Once());
      }
    }

    public void Dispose()
    {
      mockItemServiceResolver.Verify(r => r("main"), Times.Once());
    }
  }
}
