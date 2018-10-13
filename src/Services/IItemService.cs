using Entities;
using System.Collections.Generic;

namespace Services
{
  public interface IItemService
  {
    IEnumerable<Item> All();
    Item Save(Item item);
    void Update(Item item);
    void RemoveById(int id);
  }
}
