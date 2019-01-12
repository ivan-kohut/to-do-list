using Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
  public class ItemRepository : BaseRepository, IItemRepository
  {
    private readonly AppDbContext dbContext;

    public ItemRepository(AppDbContext dbContext) : base(dbContext)
    {
      this.dbContext = dbContext;
    }

    public async Task<Item> GetByIdAsync(int id)
    {
      return await dbContext.Items.FindAsync(id);
    }

    public async Task<Item> GetByPriorityAsync(int priority)
    {
      return await dbContext.Items.SingleOrDefaultAsync(i => i.Priority == priority);
    }

    public async Task<int> GetMaxItemPriorityAsync()
    {
      return (await dbContext
        .Items
        .Select(i => i.Priority)
        .ToListAsync())
        .DefaultIfEmpty(0)
        .Max();
    }

    public IQueryable<Item> All()
    {
      return dbContext
        .Items
        .OrderBy(i => i.Priority)
        .AsNoTracking();
    }

    public async Task CreateAsync(Item item)
    {
      await dbContext.AddAsync(item);
    }

    public void Update(Item item)
    {
      dbContext.Update(item);
    }

    public void Delete(Item item)
    {
      dbContext.Remove(item);
    }
  }
}
