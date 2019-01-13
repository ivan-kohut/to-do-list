using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
  public class ItemService : IItemService
  {
    private readonly IItemRepository itemRepository;

    public ItemService(IItemRepository itemRepository)
    {
      this.itemRepository = itemRepository;
    }

    public async Task<IEnumerable<ItemDTO>> AllAsync()
    {
      return await itemRepository.All()
        .Select(i => new ItemDTO { Id = i.Id, Text = i.Text, Priority = i.Priority })
        .ToListAsync();
    }

    public async Task<ItemDTO> SaveAsync(ItemDTO itemDTO)
    {
      int maxPriority = await itemRepository.GetMaxItemPriorityAsync();

      Item item = new Item { Text = itemDTO.Text, Priority = maxPriority + 1 };

      await itemRepository.CreateAsync(item);
      await itemRepository.SaveChangesAsync();

      itemDTO.Id = item.Id;
      itemDTO.Priority = item.Priority;

      return itemDTO;
    }

    public async Task UpdatePartiallyAsync(int id, ICollection<PatchDTO> patches)
    {
      if (patches == null)
        throw new ArgumentException("Collection of PatchDTO must not be null");

      Item item = await itemRepository.GetByIdAsync(id);

      if (item == null)
        throw new EntityNotFoundException($"Item with id {id} is not found");

      itemRepository.UpdatePartially(item, patches.ToDictionary(p => p.Name, p => p.Value));

      await itemRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
      Item item = await itemRepository.GetByIdAsync(id);

      if (item == null)
        throw new EntityNotFoundException($"Item with id {id} is not found");

      itemRepository.Delete(item);

      await itemRepository.SaveChangesAsync();
    }
  }
}
