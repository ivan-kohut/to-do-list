using System.Collections.Generic;
using WebApplication.Entities;

namespace WebApplication.Services
{
  public interface IItemService
  {
    IEnumerable<Item> All();
    Item Save(Item item);
    void Update(Item item);
    void RemoveById(int id);
  }
}
