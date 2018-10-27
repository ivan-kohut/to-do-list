using Controllers.Models;
using Microsoft.AspNetCore.Mvc;
using Services;

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
    public IActionResult All()
    {
      return Json(itemService.All());
    }

    [HttpPost]
    public IActionResult Save([FromBody] ItemApiModel item)
    {
      if (!ModelState.IsValid)
        return BadRequest();

      return Json(itemService.Save(new ItemDTO { Text = item.Text }));
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] ItemApiModel item)
    {
      if (!ModelState.IsValid)
        return BadRequest();

      return GenerateResponse(itemService.Update(new ItemDTO { Id = id, Text = item.Text }));
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
      return GenerateResponse(itemService.Delete(id));
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
