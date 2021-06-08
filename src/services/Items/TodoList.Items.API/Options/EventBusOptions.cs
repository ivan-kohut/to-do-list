namespace TodoList.Items.API.Options
{
  public class EventBusOptions
  {
    public string Connection { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string BrokerName { get; set; } = null!;
    public string ClientName { get; set; } = null!;
    public int? RetryCount { get; set; } = null!;
  }
}
