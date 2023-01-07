using Microsoft.AspNetCore.Builder;
using TodoList.Items.API.Middlewares;

namespace TodoList.Items.API.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseAppExceptionHandler(this IApplicationBuilder app) =>
            app.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
