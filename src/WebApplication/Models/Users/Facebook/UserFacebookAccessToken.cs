using Newtonsoft.Json;

namespace Models
{
  public class UserFacebookAccessToken
  {
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
  }
}
