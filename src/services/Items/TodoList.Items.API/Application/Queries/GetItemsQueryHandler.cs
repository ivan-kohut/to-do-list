using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Items.API.Application.Exceptions;
using TodoList.Items.API.Application.Models;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;

namespace TodoList.Items.API.Application.Queries
{
  public class GetItemsQueryHandler : IRequestHandler<GetItemsQuery, IEnumerable<ItemDTO>>
  {
    private readonly IItemRepository itemRepository;
    private readonly IUserRepository userRepository;

    public GetItemsQueryHandler(
      IItemRepository itemRepository,
      IUserRepository userRepository)
    {
      this.itemRepository = itemRepository;
      this.userRepository = userRepository;
    }

    public async Task<IEnumerable<ItemDTO>> Handle(GetItemsQuery request, CancellationToken cancellationToken)
    {
      User? currentUser = await userRepository.GetUserAsync(request.IdentityId);

      if (currentUser is null)
      {
        throw new EntityNotFoundException($"User with identity id {request.IdentityId} is not found");
      }

      return (await itemRepository
        .GetAllAsync(currentUser.Id))
        .Select(i => new ItemDTO(i.Id, i.IsDone, i.Text, i.Priority))
        .ToList();
    }
  }
}
