using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
  public interface IItemService
  {
    Task<IEnumerable<ItemDTO>> GetAllAsync(int userId);
    Task<ItemDTO> SaveAsync(ItemDTO item);
    Task UpdateAsync(int userId, ItemDTO item);
    Task DeleteAsync(int id, int userId);
  }
}
