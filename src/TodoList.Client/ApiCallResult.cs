using System.Collections.Generic;

namespace TodoList.Client
{
  public class ApiCallResult<T>
  {
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public T Value { get; set; }
    public IEnumerable<string> Errors { get; set; }
  }
}
