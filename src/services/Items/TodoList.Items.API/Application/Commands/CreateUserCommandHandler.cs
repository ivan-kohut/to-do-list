using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Domain.Shared;

namespace TodoList.Items.API.Application.Commands
{
    public class CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository) : IRequestHandler<CreateUserCommand>
    {
        public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (await userRepository.GetUserAsync(request.IdentityId) is not null)
            {
                throw new ArgumentException($"User with the identity id already exists: {request.IdentityId}");
            }

            userRepository.Create(new User(request.IdentityId));

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
