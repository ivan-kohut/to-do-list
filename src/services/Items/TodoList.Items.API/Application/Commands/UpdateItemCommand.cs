using MediatR;

namespace TodoList.Items.API.Application.Commands
{
    public class UpdateItemCommand(int itemId, bool isDone, string text, int priority, int identityId) : ItemCommandBase(identityId), IRequest<Unit>
    {
        public int ItemId { get; } = itemId;

        public bool IsDone { get; } = isDone;

        public string Text { get; } = text;

        public int Priority { get; } = priority;
    }
}
