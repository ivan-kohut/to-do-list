using Newtonsoft.Json;

namespace Models
{
  public class UserExternalLoginAccessToken
  {
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
  }
}
