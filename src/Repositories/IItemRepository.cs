using Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
  public interface IItemRepository
  {
    Task<Item> GetByIdAndUserIdAsync(int id, int userId);
    Task<int> GetMaxItemPriorityAsync(int userId);
    IQueryable<Item> All(int userId);
    Task CreateAsync(Item item);
    void Delete(Item item);
  }
}
