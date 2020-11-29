using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
  public interface IItemService
  {
    Task<IEnumerable<ItemDTO>> GetAllAsync(int identityId);
    Task<ItemDTO> SaveAsync(int identityId, ItemDTO item);
    Task UpdateAsync(int identityId, ItemDTO item);
    Task DeleteAsync(int id, int identityId);
  }
}
