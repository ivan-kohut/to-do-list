using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Controllers
{
  [ApiController]
  [Authorize(Roles = "user")]
  [Produces("application/json")]
  [Route(Urls.Items)]
  public class ItemsController : Controller
  {
    private readonly IItemService itemService;

    public ItemsController(IItemService itemService)
    {
      this.itemService = itemService;
    }

    /// <response code="401">If user does not have role "user"</response> 
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

    /// <response code="401">If user does not have role "user"</response> 
    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
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

    /// <response code="401">If user does not have role "user"</response> 
    /// <response code="404">If item is not found by id</response> 
    [HttpPatch("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> PatchAsync(int id, [FromBody]ICollection<PatchDTO> patches)
    {
      await itemService.UpdatePartiallyAsync(id, GetUserId(), patches);

      return Ok();
    }

    /// <response code="401">If user does not have role "user"</response> 
    /// <response code="404">If item is not found by id</response> 
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
      await itemService.DeleteAsync(id, GetUserId());

      return Ok();
    }

    private int GetUserId()
    {
      return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }
  }
}
