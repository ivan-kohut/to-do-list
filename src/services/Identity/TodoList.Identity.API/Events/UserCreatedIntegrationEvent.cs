using Newtonsoft.Json;

namespace TodoList.Identity.API.Events
{
    public class UserCreatedIntegrationEvent : IIntegrationEvent
    {
        [JsonProperty("user_id")]
        public int UserId { get; private set; }

        public UserCreatedIntegrationEvent(int userId)
        {
            this.UserId = userId;
        }
    }
}
