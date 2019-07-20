using Delegates;
using Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Controllers.Services
{
  public class CachedItemService : IItemService
  {
    private readonly IItemService itemService;

    public CachedItemService(ItemServiceResolver itemServiceResolver)
    {
      this.itemService = itemServiceResolver("main");
    }

    public Task<IEnumerable<ItemDTO>> GetAllAsync(int userId)
    {
      return itemService.GetAllAsync(userId);
    }

    public Task<ItemDTO> SaveAsync(ItemDTO item)
    {
      return itemService.SaveAsync(item);
    }

    public Task UpdateAsync(int userId, ItemDTO item)
    {
      return itemService.UpdateAsync(userId, item);
    }

    public Task DeleteAsync(int id, int userId)
    {
      return itemService.DeleteAsync(id, userId);
    }
  }
}
