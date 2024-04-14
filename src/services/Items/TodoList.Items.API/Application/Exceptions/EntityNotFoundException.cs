using System;

namespace TodoList.Items.API.Application.Exceptions
{
    public class EntityNotFoundException(string message) : Exception(message)
    {
    }
}
