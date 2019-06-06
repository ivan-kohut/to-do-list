using Microsoft.AspNetCore.Http;
using Services.Exceptions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Middlewares
{
  public class ExceptionHandlerMiddleware
  {
    private readonly RequestDelegate next;

    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
      this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
      try
      {
        await next.Invoke(context);
      }
      catch (EntityNotFoundException)
      {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
      }
      catch (ArgumentException)
      {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      }
    }
  }
}
