using Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
  public interface IItemRepository : IRepository
  {
    Task<Item> GetByIdAsync(int id);
    Task<Item> GetByPriorityAsync(int priority);
    Task<int> GetMaxItemPriorityAsync();
    IQueryable<Item> All();
    Task CreateAsync(Item item);
    void Update(Item item);
    void Delete(Item item);
  }
}
