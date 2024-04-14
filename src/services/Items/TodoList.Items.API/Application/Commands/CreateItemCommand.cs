using MediatR;
using TodoList.Items.API.Application.Models;

namespace TodoList.Items.API.Application.Commands
{
    public class CreateItemCommand(string text, int identityId) : ItemCommandBase(identityId), IRequest<ItemDTO>
    {
        public string Text { get; } = text;
    }
}
