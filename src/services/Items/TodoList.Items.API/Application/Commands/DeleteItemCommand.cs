using MediatR;

namespace TodoList.Items.API.Application.Commands
{
    public class DeleteItemCommand(int itemId, int identityId) : ItemCommandBase(identityId), IRequest<Unit>
    {
        public int ItemId { get; } = itemId;
    }
}
