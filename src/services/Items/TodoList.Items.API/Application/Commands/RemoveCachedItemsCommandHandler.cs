using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System.Threading;
using System.Threading.Tasks;

namespace TodoList.Items.API.Application.Commands
{
    public class RemoveCachedItemsCommandHandler<T, R>(IMediator mediator, IMemoryCache memoryCache)
        : IRequestHandler<RemoveCachedItemsCommand<T, R>, R> where T : ItemCommandBase, IRequest<R>
    {
        public async Task<R> Handle(RemoveCachedItemsCommand<T, R> request, CancellationToken cancellationToken)
        {
            R result = await mediator.Send(request.Command, cancellationToken);

            memoryCache.Remove(request.Command.IdentityId);

            return result;
        }
    }
}
