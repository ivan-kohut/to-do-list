using Entities;
using Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
  public class ItemService : IItemService
  {
    private readonly IItemRepository itemRepository;
    private readonly IDbTransactionManager dbTransactionManager;

    public ItemService(IItemRepository itemRepository, IDbTransactionManager dbTransactionManager)
    {
      this.itemRepository = itemRepository;
      this.dbTransactionManager = dbTransactionManager;
    }

    public IEnumerable<ItemDTO> All()
    {
      return itemRepository.All()
        .Select(i => new ItemDTO { Id = i.Id, Text = i.Text })
        .ToList();
    }

    public ItemDTO Save(ItemDTO itemDTO)
    {
      Item item = new Item { Text = itemDTO.Text };

      itemRepository.Create(item);
      dbTransactionManager.SaveChanges();

      itemDTO.Id = item.Id;

      return itemDTO;
    }

    public OperationResultDTO Update(ItemDTO itemDTO)
    {
      OperationResultDTO updateOperationResult = new OperationResultDTO();

      Item item = itemRepository.GetById(itemDTO.Id);

      if (item == null)
        return updateOperationResult;

      item.Text = itemDTO.Text;

      itemRepository.Update(item);
      dbTransactionManager.SaveChanges();

      updateOperationResult.Success = true;

      return updateOperationResult;
    }

    public OperationResultDTO Delete(int id)
    {
      OperationResultDTO deleteOperationResult = new OperationResultDTO();

      Item item = itemRepository.GetById(id);

      if (item == null)
        return deleteOperationResult;

      itemRepository.Delete(item);
      dbTransactionManager.SaveChanges();

      deleteOperationResult.Success = true;

      return deleteOperationResult;
    }
  }
}
