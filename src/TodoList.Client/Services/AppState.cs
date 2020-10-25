namespace TodoList.Client
{
  public class AppState
  {
    public bool IsUserLoggedIn => true;

    public string UserName
    {
      get
      {
        return "mock";
      }
    }

    public bool IsAdmin
    {
      get
      {
        return true;
      }
    }
  }
}
