using MediatR;

namespace TodoList.Items.API.Application.Commands
{
  public class DeleteItemCommand : ItemCommandBase, IRequest
  {
    public int ItemId { get; }

    public DeleteItemCommand(int itemId, int identityId) : base(identityId)
    {
      this.ItemId = itemId;
    }
  }
}
