using MediatR;
using TodoList.Items.API.Application.Models;

namespace TodoList.Items.API.Application.Commands
{
  public class CreateItemCommand : IRequest<ItemDTO>
  {
    public string Text { get; private set; }

    public int IdentityId { get; private set; }

    public CreateItemCommand(string text, int identityId)
    {
      this.Text = text;
      this.IdentityId = identityId;
    }
  }
}
