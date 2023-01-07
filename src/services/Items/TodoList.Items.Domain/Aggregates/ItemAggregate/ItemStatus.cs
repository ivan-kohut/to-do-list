using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Domain.Aggregates.ItemAggregate
{
    public class ItemStatus : Enumeration
    {
        public static readonly ItemStatus Todo = new(1, nameof(Todo));
        public static readonly ItemStatus Done = new(2, nameof(Done));

        protected ItemStatus(int id, string name) : base(id, name)
        {
        }
    }
}
