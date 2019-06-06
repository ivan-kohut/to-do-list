namespace API.Models
{
  public class UserListApiModel
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsEmailConfirmed { get; set; }
  }
}
