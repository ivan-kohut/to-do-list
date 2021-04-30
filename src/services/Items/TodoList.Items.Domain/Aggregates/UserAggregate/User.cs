namespace TodoList.Items.Domain.Aggregates.UserAggregate
{
  public class User
  {
    private int _id;
    private int _identityId;

    public User(int identityId)
    {
      this._identityId = identityId;
    }
  }
}
