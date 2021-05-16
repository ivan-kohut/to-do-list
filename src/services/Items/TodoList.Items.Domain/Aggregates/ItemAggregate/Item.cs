using System;
using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Domain.Aggregates.ItemAggregate
{
  public class Item : Entity, IAggregateRoot
  {
    private int _userId;
    private string _text;
    private int _priority;

    public ItemStatus? Status { get; private set; }
    private int _statusId;

    public Item(int userId, string text, int priority)
    {
      this._userId = userId;
      this._statusId = ItemStatus.Todo.Id;
      this._text = !string.IsNullOrWhiteSpace(text) ? text : throw new ArgumentNullException(nameof(text));
      this._priority = priority;
    }
  }
}
