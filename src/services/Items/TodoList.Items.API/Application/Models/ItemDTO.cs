namespace TodoList.Items.API.Application.Models
{
  public class ItemDTO
  {
    public int Id { get; private set; }

    public bool IsDone { get; private set; }

    public string Text { get; private set; }

    public int Priority { get; private set; }

    public ItemDTO(int id, bool isDone, string text, int priority)
    {
      this.Id = id;
      this.IsDone = isDone;
      this.Text = text;
      this.Priority = priority;
    }
  }
}
