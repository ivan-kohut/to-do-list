namespace TodoList.Items.API.Application.Commands
{
    public abstract class ItemCommandBase(int identityId)
    {
        public int IdentityId { get; } = identityId;
    }
}
