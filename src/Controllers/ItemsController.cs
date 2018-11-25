using Controllers.Models;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Threading.Tasks;

namespace Controllers
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
    public async Task<IActionResult> AllAsync()
    {
      return Json(await itemService.AllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> SaveAsync([FromBody] ItemApiModel item)
    {
      if (!ModelState.IsValid)
        return BadRequest();

      return Json(await itemService.SaveAsync(new ItemDTO { Text = item.Text }));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] ItemApiModel item)
    {
      if (!ModelState.IsValid)
        return BadRequest();

      return GenerateResponse(await itemService.UpdateAsync(new ItemDTO { Id = id, Text = item.Text }));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
      return GenerateResponse(await itemService.DeleteAsync(id));
    }

    private IActionResult GenerateResponse(OperationResultDTO operationResult)
    {
      if (operationResult.Success)
        return Ok();
      else
        return NotFound();
    }
  }
}
