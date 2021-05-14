using System.Linq;
using System.Threading.Tasks;
using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Domain.Aggregates.ItemAggregate
{
  public interface IItemRepository : IRepository<Item>
  {
    Task<Item?> GetByIdAndIdentityIdAsync(int id, int identityId);
    Task<int?> GetMaxItemPriorityAsync(int identityId);
    IQueryable<Item> All(int identityId);
    void Create(Item item);
    void Delete(Item item);
  }
}
