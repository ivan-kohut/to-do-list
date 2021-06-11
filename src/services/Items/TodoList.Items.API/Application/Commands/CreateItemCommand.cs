using MediatR;
using TodoList.Items.API.Application.Models;

namespace TodoList.Items.API.Application.Commands
{
  public class CreateItemCommand : ItemCommandBase, IRequest<ItemDTO>
  {
    public string Text { get; }

    public CreateItemCommand(string text, int identityId) : base(identityId)
    {
      this.Text = text;
    }
  }
}
