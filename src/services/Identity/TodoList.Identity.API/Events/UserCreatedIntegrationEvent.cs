using Newtonsoft.Json;

namespace TodoList.Identity.API.Events
{
    public class UserCreatedIntegrationEvent(int userId) : IIntegrationEvent
    {
        [JsonProperty("user_id")]
        public int UserId { get; private set; } = userId;
    }
}
