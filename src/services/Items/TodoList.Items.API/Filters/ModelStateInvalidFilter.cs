using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace TodoList.Items.API.Filters
{
  public class ModelStateInvalidFilter : IActionFilter
  {
    public void OnActionExecuting(ActionExecutingContext context)
    {
      Controller currentController = (Controller)context.Controller;

      if (!currentController.ModelState.IsValid)
      {
        context.Result = currentController.BadRequest(new
        {
          errors = currentController.ModelState
            .Values
            .Where(v => v.ValidationState == ModelValidationState.Invalid)
            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage).ToList())
            .ToList()
        });
      }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
  }
}
