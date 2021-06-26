namespace TodoList.Client
{
  public class ApiUrls
  {
    private const string BaseItemsPath = "/api/v1/items";

    public const string IdTemplate = "{id}";

    public static readonly string GetItemsList = BaseItemsPath;
    public static readonly string CreateItem = BaseItemsPath;
    public static readonly string UpdateItem = BaseItemsPath + "/" + IdTemplate;
    public static readonly string DeleteItem = BaseItemsPath + "/" + IdTemplate;
  }
}
