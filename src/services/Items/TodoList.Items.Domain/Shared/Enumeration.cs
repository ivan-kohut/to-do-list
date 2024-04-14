namespace TodoList.Items.Domain.Shared
{
    public abstract class Enumeration(int id, string name)
    {
        public int Id { get; } = id;

        public string Name { get; } = name;

        public override string ToString() => Name;
    }
}
