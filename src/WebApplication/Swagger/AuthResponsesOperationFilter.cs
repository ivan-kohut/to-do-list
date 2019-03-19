using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Swagger
{
  public class AuthResponsesOperationFilter : IOperationFilter
  {
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
      IEnumerable<AuthorizeAttribute> authAttributes = context.MethodInfo.DeclaringType
        .GetCustomAttributes(true)
        .Union(context.MethodInfo.GetCustomAttributes(true))
        .OfType<AuthorizeAttribute>();

      IEnumerable<AllowAnonymousAttribute> anonymousScopes = context.MethodInfo.DeclaringType
        .GetCustomAttributes(true)
        .Union(context.MethodInfo.GetCustomAttributes(true))
        .OfType<AllowAnonymousAttribute>()
        .Distinct();

      if (authAttributes.Any() && !anonymousScopes.Any())
      {
        operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });

        operation.Security.Add(new OpenApiSecurityRequirement
        {
          {
            new OpenApiSecurityScheme
            {
              Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new List<string>()
          }
        });
      }

      IEnumerable<string> requiredScopes = context.MethodInfo
        .GetCustomAttributes(true)
        .OfType<AuthorizeAttribute>()
        .Select(attr => attr.Policy)
        .Distinct();

      if (requiredScopes.Any() && !anonymousScopes.Any())
      {
        operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
      }
    }
  }
}
