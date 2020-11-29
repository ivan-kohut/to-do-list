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

    public async Task<Item?> GetByIdAndIdentityIdAsync(int id, int identityId) =>
      await dbContext
        .Items
        .SingleOrDefaultAsync(i => i.Id == id && i.User.IdentityId == identityId);

    public async Task<int?> GetMaxItemPriorityAsync(int identityId) =>
      await dbContext
        .Items
        .Where(i => i.User.IdentityId == identityId)
        .Select(i => (int?)i.Priority)
        .MaxAsync();

    public IQueryable<Item> All(int identityId) =>
      dbContext
        .Items
        .Where(i => i.User.IdentityId == identityId)
        .OrderBy(i => i.Priority)
        .AsNoTracking();

    public void Create(Item item) => dbContext.Add(item);

    public void Delete(Item item) => dbContext.Remove(item);
  }
}
