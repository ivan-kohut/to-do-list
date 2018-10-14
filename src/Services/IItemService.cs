using System.Collections.Generic;

namespace Services
{
  public interface IItemService
  {
    IEnumerable<ItemDTO> All();
    ItemDTO Save(ItemDTO item);
    OperationResultDTO Update(ItemDTO item);
    OperationResultDTO Delete(int id);
  }
}
