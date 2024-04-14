using MediatR;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Items.API.Application.Exceptions;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Domain.Shared;

namespace TodoList.Items.API.Application.Commands
{
    public class UpdateItemCommandHandler(
        IUnitOfWork unitOfWork,
        IItemRepository itemRepository,
        IUserRepository userRepository) : IRequestHandler<UpdateItemCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
        {
            User user = await userRepository.GetUserAsync(request.IdentityId)
                ?? throw new EntityNotFoundException($"User with identity id {request.IdentityId} is not found");

            Item itemDb = await itemRepository.GetByIdAndUserIdAsync(request.ItemId, user.Id)
                ?? throw new EntityNotFoundException($"Item with id {request.ItemId} is not found");

            itemDb.Update(request.IsDone, request.Text, request.Priority);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
