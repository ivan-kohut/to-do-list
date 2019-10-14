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
      object[] customAttributes = context.MethodInfo.DeclaringType
        ?.GetCustomAttributes(true) ?? new object[] { };

      IEnumerable<AuthorizeAttribute> authAttributes = customAttributes
        .Union(context.MethodInfo.GetCustomAttributes(true))
        .OfType<AuthorizeAttribute>();

      IEnumerable<AllowAnonymousAttribute> anonymousScopes = customAttributes
        .Union(context.MethodInfo.GetCustomAttributes(true))
        .OfType<AllowAnonymousAttribute>()
        .Distinct();

      if (authAttributes.Any() && !anonymousScopes.Any())
      {
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
    }
  }
}
