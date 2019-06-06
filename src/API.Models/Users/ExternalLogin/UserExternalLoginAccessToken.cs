using Newtonsoft.Json;

namespace API.Models
{
  public class UserExternalLoginAccessToken
  {
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
  }
}
