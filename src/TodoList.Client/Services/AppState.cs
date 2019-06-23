using Blazored.LocalStorage;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace TodoList.Client
{
  public class AppState
  {
    public const string AuthTokenKey = "auth-token";
    public const string IndexUrl = "https://localhost:44328";

    private readonly ISyncLocalStorageService localStorageService;

    public AppState(ISyncLocalStorageService localStorageService)
    {
      this.localStorageService = localStorageService;
    }

    public bool IsUserLoggedIn => !string.IsNullOrWhiteSpace(localStorageService.GetItem<string>(AuthTokenKey));

    public bool IsAdmin
    {
      get
      {
        string authToken = localStorageService.GetItem<string>(AuthTokenKey);

        if (string.IsNullOrWhiteSpace(authToken))
        {
          return false;
        }

        object userRoles = JsonConvert.DeserializeObject<Dictionary<string, object>>(
          Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(authToken.Split('.')[1]))
        )[ClaimTypes.Role];

        if (userRoles is JArray)
        {
          return ((JArray)userRoles)
            .ToList()
            .Contains("admin");
        }
        else
        {
          return (string)userRoles == "admin";
        }
      }
    }
  }
}
