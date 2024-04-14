namespace TodoList.Items.API.Application.Models
{
    public class ItemDTO(int id, bool isDone, string text, int priority)
    {
        public int Id { get; } = id;

        public bool IsDone { get; } = isDone;

        public string Text { get; } = text;

        public int Priority { get; } = priority;
    }
}
