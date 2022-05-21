namespace TodoList.Identity.API.Options
{
  public class SendGridOptions
  {
    public string SenderEmail { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
  }
}
