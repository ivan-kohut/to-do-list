namespace TodoList.Identity.API.Options
{
  public class EventBusOptions
  {
    public string Connection { get; set; } = null!;
    public string BrokerName { get; set; } = null!;
  }
}
