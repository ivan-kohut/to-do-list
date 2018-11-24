using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public async Task<IEnumerable<ItemDTO>> AllAsync()
    {
      return await itemRepository.All()
        .Select(i => new ItemDTO { Id = i.Id, Text = i.Text })
        .ToListAsync();
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

      if (itemRepository.GetById(id) == null)
        return deleteOperationResult;

      itemRepository.Delete(id);
      dbTransactionManager.SaveChanges();

      deleteOperationResult.Success = true;

      return deleteOperationResult;
    }
  }
}
