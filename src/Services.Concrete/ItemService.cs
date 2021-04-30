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
    private readonly IUserRepository userRepository;
    private readonly ITransactionManager transactionManager;

    public ItemService(
      IItemRepository itemRepository,
      IUserRepository userRepository,
      ITransactionManager transactionManager)
    {
      this.itemRepository = itemRepository;
      this.userRepository = userRepository;
      this.transactionManager = transactionManager;
    }

    public async Task<IEnumerable<ItemDTO>> GetAllAsync(int identityId) =>
      await itemRepository
        .All(identityId)
        .Select(i => new ItemDTO
        {
          Id = i.Id,
          IsDone = i.Status == ItemStatus.Done,
          Text = i.Text,
          Priority = i.Priority
        })
        .ToListAsync();

    public async Task<ItemDTO> SaveAsync(int identityId, ItemDTO itemDTO)
    {
      User? currentUser = await userRepository.GetUserAsync(identityId);

      if (currentUser == null)
      {
        throw new EntityNotFoundException($"User with identity id {identityId} is not found");
      }

      Item item = new()
      {
        UserId = currentUser.Id,
        Text = itemDTO.Text,
        Status = ItemStatus.Todo,
        Priority = (await itemRepository.GetMaxItemPriorityAsync(identityId) ?? 0) + 1
      };

      itemRepository.Create(item);

      await transactionManager.SaveChangesAsync();

      itemDTO.Id = item.Id;
      itemDTO.IsDone = false;
      itemDTO.Priority = item.Priority;

      return itemDTO;
    }

    public async Task UpdateAsync(int identityId, ItemDTO item)
    {
      Item? itemDb = await itemRepository.GetByIdAndIdentityIdAsync(item.Id, identityId);

      if (itemDb == null)
      {
        throw new EntityNotFoundException($"Item with id {item.Id} is not found");
      }

      itemDb.Status = item.IsDone ? ItemStatus.Done : ItemStatus.Todo;
      itemDb.Text = item.Text;
      itemDb.Priority = item.Priority;

      await transactionManager.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int identityId)
    {
      Item? item = await itemRepository.GetByIdAndIdentityIdAsync(id, identityId);

      if (item == null)
      {
        throw new EntityNotFoundException($"Item with id {id} is not found");
      }

      itemRepository.Delete(item);

      await transactionManager.SaveChangesAsync();
    }
  }
}
