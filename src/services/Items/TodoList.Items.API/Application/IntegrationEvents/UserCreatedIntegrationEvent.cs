using Newtonsoft.Json;

namespace TodoList.Items.API.Application.IntegrationEvents
{
  public class UserCreatedIntegrationEvent : IIntegrationEvent
  {
    [JsonProperty("user_id")]
    public int UserId { get; private set; }
  }
}
