using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace Controllers
{
  public abstract class ControllerBase : Controller
  {
    protected IActionResult BadRequestWithErrors()
    {
      return BadRequest(new
      {
        errors = ModelState
          .Values
          .Where(v => v.ValidationState == ModelValidationState.Invalid)
          .SelectMany(v => v.Errors.Select(e => e.ErrorMessage).ToList())
          .ToList()
      });
    }
  }
}
