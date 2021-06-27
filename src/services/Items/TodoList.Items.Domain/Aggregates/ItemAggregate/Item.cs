using System;
using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Domain.Aggregates.ItemAggregate
{
  public class Item : Entity, IAggregateRoot
  {
    public int UserId { get; }

    public string Text { get; private set; }

    public int Priority { get; private set; }

    public ItemStatus? Status { get; }
    private int _statusId;

    public Item(int userId, string text, int priority) : this(userId, text, priority, ItemStatus.Todo)
    {
    }

    public Item(int userId, string text, int priority, ItemStatus status)
    {
      this.UserId = userId;
      this.Text = !string.IsNullOrWhiteSpace(text) ? text : throw new ArgumentNullException(nameof(text));
      this.Priority = priority;
      this._statusId = status.Id;
    }

    public bool IsDone => _statusId == ItemStatus.Done.Id;

    public void Update(bool isDone, string text, int priority)
    {
      this._statusId = (isDone ? ItemStatus.Done : ItemStatus.Todo).Id;
      this.Text = !string.IsNullOrWhiteSpace(text) ? text : throw new ArgumentNullException(nameof(text));
      this.Priority = priority;
    }
  }
}
