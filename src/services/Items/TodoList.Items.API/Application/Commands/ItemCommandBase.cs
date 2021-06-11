namespace TodoList.Items.API.Application.Commands
{
  public abstract class ItemCommandBase
  {
    public int IdentityId { get; }

    protected ItemCommandBase(int identityId)
    {
      this.IdentityId = identityId;
    }
  }
}
