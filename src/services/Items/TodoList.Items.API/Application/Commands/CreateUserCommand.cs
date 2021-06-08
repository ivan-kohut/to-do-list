using MediatR;

namespace TodoList.Items.API.Application.Commands
{
  public class CreateUserCommand : IRequest
  {
    public int IdentityId { get; private set; }

    public CreateUserCommand(int identityId)
    {
      this.IdentityId = identityId;
    }
  }
}
