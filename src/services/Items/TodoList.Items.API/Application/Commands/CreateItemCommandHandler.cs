using MediatR;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Items.API.Application.Exceptions;
using TodoList.Items.API.Application.Models;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Domain.Shared;

namespace TodoList.Items.API.Application.Commands
{
  public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, ItemDTO>
  {
    private readonly IUnitOfWork unitOfWork;
    private readonly IItemRepository itemRepository;
    private readonly IUserRepository userRepository;

    public CreateItemCommandHandler(
      IUnitOfWork unitOfWork,
      IItemRepository itemRepository,
      IUserRepository userRepository)
    {
      this.unitOfWork = unitOfWork;
      this.itemRepository = itemRepository;
      this.userRepository = userRepository;
    }

    public async Task<ItemDTO> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
      User? currentUser = await userRepository.GetUserAsync(request.IdentityId);

      if (currentUser is null)
      {
        throw new EntityNotFoundException($"User with identity id {request.IdentityId} is not found");
      }

      Item item = new(currentUser.Id, request.Text, (await itemRepository.GetMaxItemPriorityAsync(currentUser.Id) ?? 0) + 1);

      itemRepository.Create(item);

      await unitOfWork.SaveChangesAsync(cancellationToken);

      return new ItemDTO(item.Id, item.IsDone, item.Text, item.Priority);
    }
  }
}
