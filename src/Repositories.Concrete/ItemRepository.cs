using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
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

    public async Task<Item> GetByIdAndUserIdAsync(int id, int userId)
    {
      return await dbContext
        .Items
        .SingleOrDefaultAsync(i => i.Id == id && i.UserId == userId);
    }

    public async Task<int> GetMaxItemPriorityAsync(int userId)
    {
      return (await dbContext
        .Items
        .Where(i => i.UserId == userId)
        .Select(i => i.Priority)
        .ToListAsync())
        .DefaultIfEmpty(0)
        .Max();
    }

    public IQueryable<Item> All(int userId)
    {
      return dbContext
        .Items
        .Where(i => i.UserId == userId)
        .OrderBy(i => i.Priority)
        .AsNoTracking();
    }

    public async Task CreateAsync(Item item)
    {
      await dbContext.AddAsync(item);
    }

    public void UpdatePartially(Item item, IDictionary<string, object> fieldsToUpdate)
    {
      EntityEntry<Item> entityEntry = dbContext.Entry(item);

      entityEntry.CurrentValues.SetValues(fieldsToUpdate);
      entityEntry.State = EntityState.Modified;
    }

    public void Delete(Item item)
    {
      dbContext.Remove(item);
    }
  }
}
