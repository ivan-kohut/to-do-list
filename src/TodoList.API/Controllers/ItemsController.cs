using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
  [ProducesResponseType(200)]
  [ProducesResponseType(401)]
  [Route(Urls.Items)]
  public class ItemsController : Controller
  {
    private readonly IItemService itemService;

    public ItemsController(IItemService itemService)
    {
      this.itemService = itemService;
    }

    /// <response code="403">If user does not have role "user"</response> 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemApiModel>>> GetAllAsync()
    {
      return (await itemService.GetAllAsync(GetUserId()))
        .Select(i => new ItemApiModel
        {
          Id = i.Id,
          IsDone = i.IsDone,
          Text = i.Text,
          Priority = i.Priority
        })
        .ToList();
    }

    /// <response code="403">If user does not have role "user"</response> 
    [HttpPost]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ItemApiModel>> SaveAsync(ItemCreateApiModel item)
    {
      ItemDTO result = await itemService.SaveAsync(new ItemDTO { UserId = GetUserId(), Text = item.Text });

      return new ItemApiModel
      {
        Id = result.Id,
        IsDone = result.IsDone,
        Text = result.Text,
        Priority = result.Priority
      };
    }

    /// <response code="403">If user does not have role "user"</response> 
    /// <response code="404">If item is not found by id</response> 
    [HttpPut(Urls.UpdateItem)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> PutAsync(int id, [FromBody] ItemApiModel item)
    {
      if (id != item.Id)
      {
        return BadRequest();
      }

      await itemService.UpdateAsync(GetUserId(), new ItemDTO
      {
        Id = item.Id,
        IsDone = item.IsDone,
        Text = item.Text,
        Priority = item.Priority
      });

      return Ok();
    }

    /// <response code="403">If user does not have role "user"</response> 
    /// <response code="404">If item is not found by id</response> 
    [HttpDelete(Urls.DeleteItem)]
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
