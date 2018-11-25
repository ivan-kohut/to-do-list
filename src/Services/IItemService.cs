using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
  public interface IItemService
  {
    Task<IEnumerable<ItemDTO>> AllAsync();
    Task<ItemDTO> SaveAsync(ItemDTO item);
    Task<OperationResultDTO> UpdateAsync(ItemDTO item);
    Task<OperationResultDTO> DeleteAsync(int id);
  }
}
