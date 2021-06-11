using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Domain.Aggregates.UserAggregate
{
  public class User : Entity, IAggregateRoot
  {
    public int IdentityId { get; private set; }

    public User(int identityId)
    {
      this.IdentityId = identityId;
    }

    public void SetIdentityId(int identityId) => this.IdentityId = identityId;
  }
}
