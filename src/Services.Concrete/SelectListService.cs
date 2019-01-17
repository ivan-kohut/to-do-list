using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
  public class SelectListService : ISelectListService
  {
    public IEnumerable<SelectListItemDTO> BuildSelectList<T>() where T : struct
    {
      Type structType = typeof(T);

      if (!structType.IsEnum)
      {
        throw new ArgumentException($"Struct with type {structType.ToString()} is not enum");
      }

      return Enum.GetValues(structType)
        .Cast<T>()
        .Select(s => new SelectListItemDTO
        {
          Value = ((int)(object)s).ToString(),
          Text = s.ToString()
        })
        .ToList();
    }
  }
}
