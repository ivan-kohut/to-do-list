using Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
  public interface IItemRepository : IRepository
  {
    Task<Item> GetByIdAndUserIdAsync(int id, int userId);
    Task<int> GetMaxItemPriorityAsync(int userId);
    IQueryable<Item> All(int userId);
    Task CreateAsync(Item item);
    void UpdatePartially(Item item, IDictionary<string, object> fieldsToUpdate);
    void Delete(Item item);
  }
}
