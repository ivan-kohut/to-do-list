using Microsoft.AspNetCore.Mvc;
using WebApplication.Entities;
using WebApplication.Services;

namespace WebApplication.Controllers.Api
{
  [Route("/[controller]")]
  public class ItemsController : Controller
  {
    private readonly IItemService itemService;

    public ItemsController(IItemService itemService)
    {
      this.itemService = itemService;
    }

    [HttpGet]
    public IActionResult All()
    {
      return this.Json(itemService.All());
    }

    [HttpPost]
    public IActionResult Save([FromBody] Item item)
    {
      if (this.IsEmpty(item.Text))
        return this.BadRequest();

      return this.Json(itemService.Save(item));
    }

    [HttpPut]
    public void Update([FromBody] Item item)
    {
      if (!this.IsEmpty(item.Text))
        itemService.Update(item);
    }

    [HttpDelete("{id}")]
    public void Delete(int id)
    {
      itemService.RemoveById(id);
    }

    private bool IsEmpty(string str) => str.Length == 0;
  }
}
