using Delegates;
using Microsoft.Extensions.Caching.Memory;
using Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Controllers.Services
{
  public class CachedItemService : IItemService
  {
    private readonly IItemService itemService;
    private readonly IMemoryCache memoryCache;

    public CachedItemService(ItemServiceResolver itemServiceResolver, IMemoryCache memoryCache)
    {
      this.itemService = itemServiceResolver("main");
      this.memoryCache = memoryCache;
    }

    public async Task<IEnumerable<ItemDTO>> GetAllAsync(int userId)
    {
      if (!memoryCache.TryGetValue(userId, out IEnumerable<ItemDTO> userItems))
      {
        userItems = await itemService.GetAllAsync(userId);

        MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
          .SetSlidingExpiration(TimeSpan.FromMinutes(1));

        memoryCache.Set(userId, userItems, cacheEntryOptions);
      }

      return userItems;
    }

    public async Task<ItemDTO> SaveAsync(ItemDTO item)
    {
      ItemDTO savedItem = await itemService.SaveAsync(item);

      memoryCache.Remove(item.UserId);

      return savedItem;
    }

    public async Task UpdateAsync(int userId, ItemDTO item)
    {
      await itemService.UpdateAsync(userId, item);

      memoryCache.Remove(userId);
    }

    public async Task DeleteAsync(int id, int userId)
    {
      await itemService.DeleteAsync(id, userId);

      memoryCache.Remove(userId);
    }
  }
}
