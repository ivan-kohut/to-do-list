using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;
using Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Controllers
{
  [ApiController]
  [Authorize(Roles = "user")]
  [Route("/api/v1/[controller]")]
  public class ItemsController : Controller
  {
    private readonly IItemService itemService;

    public ItemsController(IItemService itemService)
    {
      this.itemService = itemService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDTO>>> GetAllAsync()
    {
      return new List<ItemDTO>(await itemService.GetAllAsync(GetUserId()));
    }

    [HttpPost]
    public async Task<ActionResult<ItemDTO>> SaveAsync(ItemCreateApiModel item)
    {
      return await itemService.SaveAsync(new ItemDTO { UserId = GetUserId(), Text = item.Text });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchAsync(int id, [FromBody]ICollection<PatchDTO> patches)
    {
      IActionResult actionResult = Ok();

      try
      {
        await itemService.UpdatePartiallyAsync(id, GetUserId(), patches);
      }
      catch (EntityNotFoundException)
      {
        actionResult = NotFound();
      }
      catch (ArgumentException)
      {
        actionResult = BadRequest();
      }

      return actionResult;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
      IActionResult actionResult = Ok();

      try
      {
        await itemService.DeleteAsync(id, GetUserId());
      }
      catch (EntityNotFoundException)
      {
        actionResult = NotFound();
      }

      return actionResult;
    }

    private int GetUserId()
    {
      return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }
  }
}
