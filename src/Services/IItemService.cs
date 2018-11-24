using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
  public interface IItemService
  {
    Task<IEnumerable<ItemDTO>> AllAsync();
    ItemDTO Save(ItemDTO item);
    OperationResultDTO Update(ItemDTO item);
    OperationResultDTO Delete(int id);
  }
}
