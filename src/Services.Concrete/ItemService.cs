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

    public async Task<ItemDTO> SaveAsync(ItemDTO itemDTO)
    {
      Item item = new Item { Text = itemDTO.Text };

      await itemRepository.CreateAsync(item);
      await dbTransactionManager.SaveChangesAsync();

      itemDTO.Id = item.Id;

      return itemDTO;
    }

    public async Task<OperationResultDTO> UpdateAsync(ItemDTO itemDTO)
    {
      OperationResultDTO updateOperationResult = new OperationResultDTO();

      Item item = await itemRepository.GetByIdAsync(itemDTO.Id);

      if (item == null)
        return updateOperationResult;

      item.Text = itemDTO.Text;

      itemRepository.Update(item);

      await dbTransactionManager.SaveChangesAsync();

      updateOperationResult.Success = true;

      return updateOperationResult;
    }

    public async Task<OperationResultDTO> DeleteAsync(int id)
    {
      OperationResultDTO deleteOperationResult = new OperationResultDTO();

      Item item = await itemRepository.GetByIdAsync(id);

      if (item == null)
        return deleteOperationResult;

      itemRepository.Delete(item);

      await dbTransactionManager.SaveChangesAsync();

      deleteOperationResult.Success = true;

      return deleteOperationResult;
    }
  }
}
