﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
  public interface IItemService
  {
    Task<IEnumerable<ItemDTO>> AllAsync();
    Task<ItemDTO> SaveAsync(ItemDTO item);
    Task UpdatePartiallyAsync(int id, ICollection<PatchDTO> patches);
    Task DeleteAsync(int id);
  }
}
