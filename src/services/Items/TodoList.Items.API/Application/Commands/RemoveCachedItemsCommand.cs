using MediatR;

namespace TodoList.Items.API.Application.Commands
{
    public class RemoveCachedItemsCommand<T, R>(T command) : IRequest<R> where T : ItemCommandBase, IRequest<R>
    {
        public T Command { get; } = command;
    }
}
