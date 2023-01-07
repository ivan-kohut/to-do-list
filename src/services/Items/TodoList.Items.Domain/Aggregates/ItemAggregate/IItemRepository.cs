using System.Collections.Generic;
using System.Threading.Tasks;
using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Domain.Aggregates.ItemAggregate
{
    public interface IItemRepository : IRepository<Item>
    {
        Task<IEnumerable<Item>> GetAllAsync(int userId);

        Task<Item?> GetByIdAndUserIdAsync(int id, int userId);

        Task<int?> GetMaxItemPriorityAsync(int userId);

        void Delete(Item item);
    }
}
