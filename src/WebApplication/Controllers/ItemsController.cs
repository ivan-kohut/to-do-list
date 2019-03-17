using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;
using Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public async Task<ActionResult<IEnumerable<ItemListApiModel>>> GetAllAsync()
    {
      return (await itemService.GetAllAsync(GetUserId()))
        .Select(i => new ItemListApiModel
        {
          Id = i.Id,
          StatusId = i.StatusId,
          Text = i.Text,
          Priority = i.Priority
        })
        .ToList();
    }

    [HttpPost]
    public async Task<ActionResult<ItemListApiModel>> SaveAsync(ItemCreateApiModel item)
    {
      ItemDTO result = await itemService.SaveAsync(new ItemDTO { UserId = GetUserId(), Text = item.Text });

      return new ItemListApiModel
      {
        Id = result.Id,
        StatusId = result.StatusId,
        Text = result.Text,
        Priority = result.Priority
      };
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
