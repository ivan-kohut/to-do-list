using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Domain.Aggregates.UserAggregate
{
  public class User : Entity, IAggregateRoot
  {
    private int _identityId;

    public User(int identityId)
    {
      this._identityId = identityId;
    }
  }
}
