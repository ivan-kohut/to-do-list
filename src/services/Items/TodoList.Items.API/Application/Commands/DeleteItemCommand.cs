using MediatR;

namespace TodoList.Items.API.Application.Commands
{
  public class DeleteItemCommand : IRequest
  {
    public int ItemId { get; private set; }

    public int IdentityId { get; private set; }

    public DeleteItemCommand(int itemId, int identityId)
    {
      this.ItemId = itemId;
      this.IdentityId = identityId;
    }
  }
}
