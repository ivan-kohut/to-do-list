using API.Models;

namespace TodoList.Client
{
  public class ApiUrls
  {
    public static readonly string GetItemsList = Urls.Items;
    public static readonly string CreateItem = Urls.Items;
    public static readonly string UpdateItem = $"{Urls.Items}/{Urls.UpdateItem}";
    public static readonly string DeleteItem = $"{Urls.Items}/{Urls.DeleteItem}";
  }
}
