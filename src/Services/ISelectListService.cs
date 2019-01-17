using System.Collections.Generic;

namespace Services
{
  public interface ISelectListService
  {
    IEnumerable<SelectListItemDTO> BuildSelectList<T>() where T : struct;
  }
}
