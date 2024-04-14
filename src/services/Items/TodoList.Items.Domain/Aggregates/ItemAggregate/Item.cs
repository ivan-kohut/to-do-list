using System;
using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Domain.Aggregates.ItemAggregate
{
    public class Item(int userId, string text, int priority, ItemStatus status) : Entity, IAggregateRoot
    {
        public int UserId { get; } = userId;

        public string Text { get; private set; } = !string.IsNullOrWhiteSpace(text) ? text : throw new ArgumentNullException(nameof(text));

        public int Priority { get; private set; } = priority;

        public ItemStatus? Status { get; }
        private int _statusId = status.Id;

        public Item(int userId, string text, int priority) : this(userId, text, priority, ItemStatus.Todo)
        {
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
