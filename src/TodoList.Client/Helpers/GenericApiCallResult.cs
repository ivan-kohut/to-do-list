namespace TodoList.Client
{
  public class ApiCallResult<T> : ApiCallResult where T : class
  {
    public T? Value { get; set; }
  }
}
