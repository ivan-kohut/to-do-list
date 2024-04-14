using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Domain.Aggregates.UserAggregate
{
    public class User(int identityId) : Entity, IAggregateRoot
    {
        public int IdentityId { get; private set; } = identityId;

        public void SetIdentityId(int identityId) => this.IdentityId = identityId;
    }
}
