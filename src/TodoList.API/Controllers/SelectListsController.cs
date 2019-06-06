using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Collections.Generic;

namespace Controllers
{
  [ApiController]
  [Authorize(Roles = "user")]
  [Produces("application/json")]
  [Route("/api/v1/select-lists")]
  public class SelectListsController : Controller
  {
    private readonly ISelectListService selectListService;

    public SelectListsController(ISelectListService selectListService)
    {
      this.selectListService = selectListService;
    }

    /// <response code="401">If user does not have role "user"</response> 
    [HttpGet("item-statuses")]
    public ActionResult<IEnumerable<SelectListItemDTO>> GetStatusesSelectList()
    {
      return new List<SelectListItemDTO>(selectListService.BuildSelectList<ItemStatus>());
    }
  }
}
