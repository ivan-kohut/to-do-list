namespace TodoList.Items.Domain.Shared
{
  public abstract class Enumeration
  {
    public int Id { get; private set; }
    public string Name { get; private set; }

    protected Enumeration(int id, string name)
    {
      this.Id = id;
      this.Name = name;
    }

    public override string ToString() => Name;
  }
}
