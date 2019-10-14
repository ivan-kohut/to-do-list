using Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
  public class ItemRepository : IItemRepository
  {
    private readonly AppDbContext dbContext;

    public ItemRepository(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public async Task<Item?> GetByIdAndUserIdAsync(int id, int userId) =>
      await dbContext
        .Items
        .SingleOrDefaultAsync(i => i.Id == id && i.UserId == userId);

    public async Task<int?> GetMaxItemPriorityAsync(int userId) =>
      await dbContext
        .Items
        .Where(i => i.UserId == userId)
        .Select(i => (int?)i.Priority)
        .MaxAsync();

    public IQueryable<Item> All(int userId) =>
      dbContext
        .Items
        .Where(i => i.UserId == userId)
        .OrderBy(i => i.Priority)
        .AsNoTracking();

    public async Task CreateAsync(Item item) => await dbContext.AddAsync(item);

    public void Delete(Item item) => dbContext.Remove(item);
  }
}
