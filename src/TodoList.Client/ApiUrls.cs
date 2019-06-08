using API.Models;

namespace TodoList.Client
{
  public class ApiUrls
  {
    private const string apiUrl = "https://localhost:44388";

    public static readonly string Login = $"{apiUrl}{Urls.Users}/{Urls.Login}";
  }
}
