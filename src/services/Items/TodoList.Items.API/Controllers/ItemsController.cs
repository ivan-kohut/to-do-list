using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Items.API.Application.Commands;
using TodoList.Items.API.Application.Models;
using TodoList.Items.API.Application.Queries;
using TodoList.Items.API.Extensions;
using TodoList.Items.API.Models;

namespace TodoList.Items.API.Controllers
{
  [ApiController]
  [Authorize(Roles = "user")]
  [Route("api/v1/[controller]")]
  [Produces("application/json")]
  [ProducesResponseType(200)]
  [ProducesResponseType(401)]
  public class ItemsController : Controller
  {
    private readonly IMediator mediator;

    public ItemsController(IMediator mediator)
    {
      this.mediator = mediator;
    }

    /// <response code="403">If user does not have role "user"</response> 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemApiModel>>> GetAllAsync(CancellationToken cancellationToken) =>
      (await mediator.Send(new GetItemsQuery(User.GetIdentityId()), cancellationToken))
        .Select(i => new ItemApiModel
        {
          Id = i.Id,
          IsDone = i.IsDone,
          Text = i.Text,
          Priority = i.Priority
        })
        .ToList();

    /// <response code="403">If user does not have role "user"</response> 
    [HttpPost]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ItemApiModel>> CreateAsync(ItemCreateApiModel item, CancellationToken cancellationToken)
    {
      CreateItemCommand command = new(item.Text, User.GetIdentityId());

      ItemDTO result = await mediator.Send(new RemoveCachedItemsCommand<CreateItemCommand, ItemDTO>(command), cancellationToken);

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
    [HttpPut("{id}")]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateAsync(int id, ItemApiModel item, CancellationToken cancellationToken)
    {
      if (id != item.Id)
      {
        return BadRequest();
      }

      UpdateItemCommand command = new(item.Id, item.IsDone, item.Text, item.Priority, User.GetIdentityId());

      await mediator.Send(new RemoveCachedItemsCommand<UpdateItemCommand, Unit>(command), cancellationToken);

      return Ok();
    }

    /// <response code="403">If user does not have role "user"</response> 
    /// <response code="404">If item is not found by id</response> 
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
      DeleteItemCommand command = new(id, User.GetIdentityId());

      await mediator.Send(new RemoveCachedItemsCommand<DeleteItemCommand, Unit>(command), cancellationToken);

      return Ok();
    }
  }
}
