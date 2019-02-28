using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
  public interface IItemService
  {
    Task<IEnumerable<ItemDTO>> GetAllAsync(int userId);
    Task<ItemDTO> SaveAsync(ItemDTO item);
    Task UpdatePartiallyAsync(int id, int userId, ICollection<PatchDTO> patches);
    Task DeleteAsync(int id, int userId);
  }
}
