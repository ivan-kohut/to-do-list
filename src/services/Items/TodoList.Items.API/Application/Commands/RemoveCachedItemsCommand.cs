using MediatR;

namespace TodoList.Items.API.Application.Commands
{
  public class RemoveCachedItemsCommand<T, R> : IRequest<R> where T : ItemCommandBase, IRequest<R>
  {
    public T Command { get; }

    public RemoveCachedItemsCommand(T command)
    {
      this.Command = command;
    }
  }
}
