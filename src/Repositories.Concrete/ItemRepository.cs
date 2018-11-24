using Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Repositories
{
  public class ItemRepository : IItemRepository
  {
    private readonly AppDbContext dbContext;

    public ItemRepository(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public Item GetById(int id)
    {
      return dbContext.Items.Find(id);
    }

    public IQueryable<Item> All()
    {
      return dbContext.Items.AsNoTracking();
    }

    public void Create(Item item)
    {
      dbContext.Add(item);
    }

    public void Update(Item item)
    {
      dbContext.Update(item);
    }

    public void Delete(int id)
    {
      dbContext.Remove(GetById(id));
    }
  }
}
