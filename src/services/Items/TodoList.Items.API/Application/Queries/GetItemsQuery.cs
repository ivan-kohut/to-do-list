using MediatR;
using System.Collections.Generic;
using TodoList.Items.API.Application.Models;

namespace TodoList.Items.API.Application.Queries
{
  public class GetItemsQuery : IRequest<IEnumerable<ItemDTO>>
  {
    public int IdentityId { get; }

    public GetItemsQuery(int identityId)
    {
      this.IdentityId = identityId;
    }
  }
}
