using MediatR;

namespace TodoList.Items.API.Application.Commands
{
  public class UpdateItemCommand : ItemCommandBase, IRequest
  {
    public int ItemId { get; }

    public bool IsDone { get; }

    public string Text { get; }

    public int Priority { get; }

    public UpdateItemCommand(int itemId, bool isDone, string text, int priority, int identityId) : base(identityId)
    {
      this.ItemId = itemId;
      this.IsDone = isDone;
      this.Text = text;
      this.Priority = priority;
    }
  }
}
