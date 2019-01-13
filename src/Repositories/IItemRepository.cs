using Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
  public interface IItemRepository : IRepository
  {
    Task<Item> GetByIdAsync(int id);
    Task<int> GetMaxItemPriorityAsync();
    IQueryable<Item> All();
    Task CreateAsync(Item item);
    void UpdatePartially(Item item, IDictionary<string, object> fieldsToUpdate);
    void Delete(Item item);
  }
}
