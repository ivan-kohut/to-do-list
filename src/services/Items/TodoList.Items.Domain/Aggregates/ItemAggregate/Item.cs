using System;
using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Domain.Aggregates.ItemAggregate
{
  public class Item : Entity, IAggregateRoot
  {
    public int UserId { get; private set; }

    public string Text { get; private set; }

    public int Priority { get; private set; }

    public ItemStatus Status { get; }
    private int _statusId;

    public Item(int userId, string text, int priority)
    {
      this.UserId = userId;
      this.Text = !string.IsNullOrWhiteSpace(text) ? text : throw new ArgumentNullException(nameof(text));
      this.Priority = priority;
      this.Status = ItemStatus.Todo;
      this._statusId = ItemStatus.Todo.Id;
    }
  }
}
