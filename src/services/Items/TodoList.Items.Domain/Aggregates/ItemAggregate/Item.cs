using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Domain.Aggregates.ItemAggregate
{
  public class Item : Entity, IAggregateRoot
  {
    private int _userId;
    private string _text;
    private int _priority;

    private int _statusId;
    public ItemStatus? Status { get; private set; }

    public Item(int userId, string text, int priority)
    {
      this._userId = userId;
      this._statusId = ItemStatus.Todo.Id;
      this._text = text;
      this._priority = priority;
    }
  }
}
