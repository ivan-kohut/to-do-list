using Newtonsoft.Json;

namespace Events
{
  public class UserCreatedIntegrationEvent : IIntegrationEvent
  {
    [JsonProperty("user_id")]
    public int UserId { get; private set; }
  }
}
