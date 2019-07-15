using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
  public class ItemService : IItemService
  {
    private readonly IItemRepository itemRepository;
    private readonly ITransactionManager transactionManager;

    public ItemService(IItemRepository itemRepository, ITransactionManager transactionManager)
    {
      this.itemRepository = itemRepository;
      this.transactionManager = transactionManager;
    }

    public async Task<IEnumerable<ItemDTO>> GetAllAsync(int userId)
    {
      return await itemRepository
        .All(userId)
        .Select(i => new ItemDTO
        {
          Id = i.Id,
          IsDone = i.Status == ItemStatus.Done,
          UserId = i.UserId,
          Text = i.Text,
          Priority = i.Priority
        })
        .ToListAsync();
    }

    public async Task<ItemDTO> SaveAsync(ItemDTO itemDTO)
    {
      int? maxPriority = await itemRepository.GetMaxItemPriorityAsync(itemDTO.UserId);

      Item item = new Item
      {
        UserId = itemDTO.UserId,
        Text = itemDTO.Text,
        Status = ItemStatus.Todo,
        Priority = (maxPriority ?? 0) + 1
      };

      await itemRepository.CreateAsync(item);
      await transactionManager.SaveChangesAsync();

      itemDTO.Id = item.Id;
      itemDTO.IsDone = false;
      itemDTO.Priority = item.Priority;

      return itemDTO;
    }

    public async Task UpdateAsync(int userId, ItemDTO item)
    {
      Item itemDb = await itemRepository.GetByIdAndUserIdAsync(item.Id, userId);

      if (itemDb == null)
      {
        throw new EntityNotFoundException($"Item with id {item.Id} is not found");
      }

      itemDb.Status = item.IsDone ? ItemStatus.Done : ItemStatus.Todo;
      itemDb.Text = item.Text;
      itemDb.Priority = item.Priority;

      await transactionManager.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
      Item item = await itemRepository.GetByIdAndUserIdAsync(id, userId);

      if (item == null)
        throw new EntityNotFoundException($"Item with id {id} is not found");

      itemRepository.Delete(item);

      await transactionManager.SaveChangesAsync();
    }
  }
}
