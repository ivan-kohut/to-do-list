using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using WebApplication.Entities;

namespace WebApplication.Services
{
  public class ItemService : IItemService
  {
    private readonly AppDbContext dbContext;

    public ItemService(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public IEnumerable<Item> All()
    {
      return dbContext.Items
        .AsNoTracking()
        .ToList();
    }

    public Item Save(Item item)
    {
      dbContext.Add(item);
      dbContext.SaveChanges();

      return item;
    }

    public void Update(Item item)
    {
      dbContext.Update(item);
      dbContext.SaveChanges();
    }

    public void RemoveById(int id)
    {
      dbContext.Remove(dbContext.Items.Find(id));
      dbContext.SaveChanges();
    }
  }
}
