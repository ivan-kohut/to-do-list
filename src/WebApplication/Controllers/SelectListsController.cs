using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Collections.Generic;

namespace Controllers
{
  [ApiController]
  [Authorize(Roles = "user")]
  [Route("/api/v1/select-lists")]
  public class SelectListsController : Controller
  {
    private readonly ISelectListService selectListService;

    public SelectListsController(ISelectListService selectListService)
    {
      this.selectListService = selectListService;
    }

    [HttpGet("item-statuses")]
    public ActionResult<IEnumerable<SelectListItemDTO>> GetStatusesSelectList()
    {
      return new List<SelectListItemDTO>(selectListService.BuildSelectList<ItemStatus>());
    }
  }
}
