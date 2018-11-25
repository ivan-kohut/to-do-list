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

    public async Task<Item> GetByIdAsync(int id)
    {
      return await dbContext.Items.FindAsync(id);
    }

    public IQueryable<Item> All()
    {
      return dbContext.Items.AsNoTracking();
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
