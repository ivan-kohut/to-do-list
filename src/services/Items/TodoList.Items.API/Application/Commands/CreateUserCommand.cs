using MediatR;

namespace TodoList.Items.API.Application.Commands
{
    public class CreateUserCommand : IRequest
    {
        public int IdentityId { get; }

        public CreateUserCommand(int identityId)
        {
            this.IdentityId = identityId;
        }
    }
}
