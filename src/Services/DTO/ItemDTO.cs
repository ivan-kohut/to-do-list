namespace Services
{
  public class ItemDTO
  {
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool IsDone { get; set; }
    public string Text { get; set; } = null!;
    public int Priority { get; set; }
  }
}
