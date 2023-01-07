namespace TodoList.Items.Domain.Shared
{
    public abstract class Enumeration
    {
        public int Id { get; }

        public string Name { get; }

        protected Enumeration(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public override string ToString() => Name;
    }
}
