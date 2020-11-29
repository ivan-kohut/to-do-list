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
      this.itemService = itemServiceResolver("main") ?? throw new ArgumentNullException("Main item service");
      this.memoryCache = memoryCache;
    }

    public async Task<IEnumerable<ItemDTO>> GetAllAsync(int identityId)
    {
      if (!memoryCache.TryGetValue(identityId, out IEnumerable<ItemDTO> userItems))
      {
        userItems = await itemService.GetAllAsync(identityId);

        MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
          .SetSlidingExpiration(TimeSpan.FromMinutes(1));

        memoryCache.Set(identityId, userItems, cacheEntryOptions);
      }

      return userItems;
    }

    public async Task<ItemDTO> SaveAsync(int identityId, ItemDTO item)
    {
      ItemDTO savedItem = await itemService.SaveAsync(identityId, item);

      memoryCache.Remove(identityId);

      return savedItem;
    }

    public async Task UpdateAsync(int identityId, ItemDTO item)
    {
      await itemService.UpdateAsync(identityId, item);

      memoryCache.Remove(identityId);
    }

    public async Task DeleteAsync(int id, int identityId)
    {
      await itemService.DeleteAsync(id, identityId);

      memoryCache.Remove(identityId);
    }
  }
}
