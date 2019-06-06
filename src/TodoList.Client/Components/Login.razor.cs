using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class LoginComponent : ComponentBase
  {
    [Inject]
    private HttpClient HttpClient { get; set; }

    protected string Email { get; set; }
    protected string Password { get; set; }
    protected string TwoFactorToken { get; set; }

    protected bool IsTwoFactorTokenFieldDisplayed { get; set; }

    protected async Task OnLoginAsync()
    {
      using (HttpContent httpContent = CreateHttpContent(new { Email, Password, TwoFactorToken }))
      {
        HttpResponseMessage some  = await HttpClient.PostAsync("https://localhost:44388/api/v1/users/login", httpContent);

        Console.WriteLine(some.IsSuccessStatusCode);
        Console.WriteLine($"Email -> {Email}; Password -> {Password}");
      }
    }

    private HttpContent CreateHttpContent(object requestBody)
    {
      return new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
    }
  }
}
