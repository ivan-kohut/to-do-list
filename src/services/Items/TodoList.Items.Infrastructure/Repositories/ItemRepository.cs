using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Items.Domain.Aggregates.ItemAggregate;

namespace TodoList.Items.Infrastructure.Repositories
{
    public class ItemRepository(ItemsDbContext dbContext) : RepositoryBase<Item>(dbContext), IItemRepository
    {
        public async Task<IEnumerable<Item>> GetAllAsync(int userId) =>
            await dbContext.Items
                .Where(i => i.UserId == userId)
                .OrderBy(i => i.Priority)
                .AsNoTracking()
                .ToListAsync();

        public async Task<Item?> GetByIdAndUserIdAsync(int id, int userId) =>
            await dbContext.Items
                .SingleOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        public async Task<int?> GetMaxItemPriorityAsync(int userId) =>
            await dbContext.Items
                .Where(i => i.UserId == userId)
                .Select(i => (int?)i.Priority)
                .MaxAsync();

        public void Delete(Item item) => dbContext.Remove(item);
    }
}
