using Entities;
using System.Collections.Generic;

namespace Repositories
{
  public interface IItemRepository
  {
    Item GetById(int id);
    IEnumerable<Item> All();
    void Create(Item item);
    void Update(Item item);
    void Delete(Item item);
  }
}
