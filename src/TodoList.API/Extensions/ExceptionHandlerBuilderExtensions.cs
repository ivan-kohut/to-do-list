using Microsoft.AspNetCore.Builder;
using Middlewares;

namespace Extensions
{
  public static class ExceptionHandlerBuilderExtensions
  {
    public static IApplicationBuilder UseAppExceptionHandler(this IApplicationBuilder app) =>
      app.UseMiddleware<ExceptionHandlerMiddleware>();
  }
}
