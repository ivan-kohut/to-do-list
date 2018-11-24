using Entities;
using System.Linq;

namespace Repositories
{
  public interface IItemRepository
  {
    Item GetById(int id);
    IQueryable<Item> All();
    void Create(Item item);
    void Update(Item item);
    void Delete(int id);
  }
}
