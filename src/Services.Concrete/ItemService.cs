using Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
  public class ItemService : IItemService
  {
    private readonly AppDbContext dbContext;

    public ItemService(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public IEnumerable<ItemDTO> All()
    {
      return dbContext.Items
        .AsNoTracking()
        .Select(i => new ItemDTO { Id = i.Id, Text = i.Text })
        .ToList();
    }

    public ItemDTO Save(ItemDTO itemDTO)
    {
      Item item = new Item { Text = itemDTO.Text };

      dbContext.Add(item);
      dbContext.SaveChanges();

      itemDTO.Id = item.Id;

      return itemDTO;
    }

    public OperationResultDTO Update(ItemDTO itemDTO)
    {
      OperationResultDTO updateOperationResult = new OperationResultDTO();

      Item item = dbContext.Items.Find(itemDTO.Id);

      if (item == null)
        return updateOperationResult;

      dbContext.Update(item);
      dbContext.SaveChanges();

      updateOperationResult.Success = true;

      return updateOperationResult;
    }

    public OperationResultDTO Delete(int id)
    {
      OperationResultDTO deleteOperationResult = new OperationResultDTO();

      Item item = dbContext.Items.Find(id);

      if (item == null)
        return deleteOperationResult;

      dbContext.Remove(item);
      dbContext.SaveChanges();

      deleteOperationResult.Success = true;

      return deleteOperationResult;
    }
  }
}
