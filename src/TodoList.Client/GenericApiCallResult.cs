namespace TodoList.Client
{
  public class ApiCallResult<T> : ApiCallResult
  {
    public T Value { get; set; }
  }
}
