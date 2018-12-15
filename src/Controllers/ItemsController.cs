using Controllers.Models;
using Microsoft.AspNetCore.Mvc;
using Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Controllers
{
  [ApiController]
  [Route("/[controller]")]
  public class ItemsController : Controller
  {
    private readonly IItemService itemService;

    public ItemsController(IItemService itemService)
    {
      this.itemService = itemService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDTO>>> AllAsync()
    {
      return new List<ItemDTO>(await itemService.AllAsync());
    }

    [HttpPost]
    public async Task<ActionResult<ItemDTO>> SaveAsync(ItemApiModel item)
    {
      return await itemService.SaveAsync(new ItemDTO { Text = item.Text });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, ItemApiModel item)
    {
      try
      {
        await itemService.UpdateAsync(new ItemDTO { Id = id, Text = item.Text });

        return Ok();
      }
      catch (ArgumentException)
      {
        return NotFound();
      }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
      try
      {
        await itemService.DeleteAsync(id);

        return Ok();
      }
      catch (ArgumentException)
      {
        return NotFound();
      }
    }
  }
}