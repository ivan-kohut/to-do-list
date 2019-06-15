using System.Collections.Generic;

namespace TodoList.Client
{
  public class ApiCallResult
  {
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public IEnumerable<string> Errors { get; set; }
  }
}
