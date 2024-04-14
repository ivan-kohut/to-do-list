using MediatR;

namespace TodoList.Items.API.Application.Commands
{
    public class CreateUserCommand(int identityId) : IRequest
    {
        public int IdentityId { get; } = identityId;
    }
}
