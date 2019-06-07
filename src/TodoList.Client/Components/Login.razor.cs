using API.Models;
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
    private IAppHttpClient AppHttpClient { get; set; }

    protected UserLoginModel UserLoginModel { get; set; }
    protected bool IsTwoFactorTokenFieldDisplayed { get; set; }

    protected override void OnInit()
    {
      UserLoginModel = new UserLoginModel();
    }

    protected async Task OnLoginAsync()
    {
      HttpResponseMessage some = await AppHttpClient.PostAsync("https://localhost:44388/api/v1/users/login", UserLoginModel);

      Console.WriteLine(some.IsSuccessStatusCode);
      Console.WriteLine($"Email -> {UserLoginModel.Email}; Password -> {UserLoginModel.Password}");
    }
  }
}
