namespace TodoList.Items.Domain.Shared
{
  public interface IRepository<T> where T : IAggregateRoot
  {
    IUnitOfWork UnitOfWork { get; }
  }
}
