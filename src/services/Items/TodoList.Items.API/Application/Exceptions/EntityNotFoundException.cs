using System;

namespace TodoList.Items.API.Application.Exceptions
{
  public class EntityNotFoundException : Exception
  {
    public EntityNotFoundException(string message) : base(message)
    {
    }
  }
}
