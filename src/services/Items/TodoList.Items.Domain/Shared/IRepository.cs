namespace TodoList.Items.Domain.Shared
{
  public interface IRepository<T> where T : class, IAggregateRoot
  {
    void Create(T aggregateRoot);
  }
}
