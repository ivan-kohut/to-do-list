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
      return await itemRepository
        .All()
        .Select(i => new ItemDTO
        {
          Id = i.Id,
          StatusId = (int)i.Status,
          Text = i.Text,
          Priority = i.Priority
        })
        .ToListAsync();
    }

    public IEnumerable<SelectListItemDTO> GetStatusesSelectList()
    {
      return Enum.GetValues(typeof(ItemStatus))
        .Cast<ItemStatus>()
        .Select(s => new SelectListItemDTO
        {
          Value = ((int)s).ToString(),
          Text = s.ToString()
        })
        .ToList();
    }

    public async Task<ItemDTO> SaveAsync(ItemDTO itemDTO)
    {
      int maxPriority = await itemRepository.GetMaxItemPriorityAsync();

      Item item = new Item { Text = itemDTO.Text, Status = ItemStatus.Todo, Priority = maxPriority + 1 };

      await itemRepository.CreateAsync(item);
      await itemRepository.SaveChangesAsync();

      itemDTO.Id = item.Id;
      itemDTO.StatusId = (int)item.Status;
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

      CorrectPatchDTOs(patches);

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

    private void CorrectPatchDTOs(ICollection<PatchDTO> patches)
    {
      const string statusPropertyName = nameof(ItemDTO.StatusId);
      const string priorityPropertyName = nameof(ItemDTO.Priority);

      foreach (PatchDTO patchDTO in patches)
      {
        switch (patchDTO.Name)
        {
          case statusPropertyName:
            patchDTO.Name = "Status";
            patchDTO.Value = (int)(long)patchDTO.Value;
            break;
          case priorityPropertyName:
            patchDTO.Value = (int)(long)patchDTO.Value;
            break;
        }
      }
    }
  }
}
