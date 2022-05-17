using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Identity.API.Consts;
using TodoList.Identity.API.Data;
using TodoList.Identity.API.Data.Entities;
using TodoList.Identity.API.ViewModels;

namespace TodoList.Identity.API.Controllers
{
  [Authorize(Roles = adminRole)]
  public class AdminController : Controller
  {
    private const string adminRole = "admin";

    private readonly AppDbContext dbContext;

    public AdminController(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Users(CancellationToken cancellationToken)
    {
      var users = await dbContext
        .Users
        .Where(u => !u.UserRoles!.Any(ur => ur.Role!.Name == adminRole))
        .Select(u => new
        {
          u.Id,
          Name = u.UserName,
          u.Email,
          IsRegisteredInSystem = u.PasswordHash != null,
          IsEmailConfirmed = u.EmailConfirmed,
          LoginProviders = u.UserLogins
            !.Select(l => l.LoginProvider)
            .ToList()
        })
        .ToListAsync(cancellationToken);

      return View(new UsersViewModel
      {
        Users = users
          .Select(u => new UserViewModel
          {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            IsRegisteredInSystem = u.IsRegisteredInSystem,
            IsLoggedInViaFacebook = u.LoginProviders.Contains(ExternalProviderNames.Facebook),
            IsLoggedInViaGoogle = u.LoginProviders.Contains(ExternalProviderNames.Google),
            IsLoggedInViaGithub = u.LoginProviders.Contains(ExternalProviderNames.GitHub),
            IsEmailConfirmed = u.IsEmailConfirmed
          })
          .ToList()
      });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserAsync(int? id, CancellationToken cancellationToken)
    {
      if (id.HasValue)
      {
        User userToDelete = await dbContext
          .Users
          .Where(u => u.Id == id && !u.UserRoles!.Any(ur => ur.Role!.Name == adminRole))
          .SingleOrDefaultAsync(cancellationToken);

        if (userToDelete != default)
        {
          dbContext.Remove(userToDelete);

          await dbContext.SaveChangesAsync(cancellationToken);
        }
      }

      return Redirect("/admin/users");
    }
  }
}
