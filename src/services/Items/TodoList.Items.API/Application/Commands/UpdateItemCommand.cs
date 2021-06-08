using MediatR;

namespace TodoList.Items.API.Application.Commands
{
  public class UpdateItemCommand : IRequest
  {
    public int ItemId { get; private set; }

    public bool IsDone { get; private set; }

    public string Text { get; private set; }

    public int Priority { get; private set; }

    public int IdentityId { get; private set; }

    public UpdateItemCommand(int itemId, bool isDone, string text, int priority, int identityId)
    {
      this.ItemId = itemId;
      this.IsDone = isDone;
      this.Text = text;
      this.Priority = priority;
      this.IdentityId = identityId;
    }
  }
}
