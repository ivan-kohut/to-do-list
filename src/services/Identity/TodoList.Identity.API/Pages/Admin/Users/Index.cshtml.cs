using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Identity.API.Consts;
using TodoList.Identity.API.Services.Interfaces;

namespace TodoList.Identity.API.Pages.Admin.Users
{
    [Authorize(Roles = "admin")]
    public class IndexPageModel(IUserService userService) : PageModel
    {
        public IEnumerable<UserViewModel>? Users { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Users = (await userService.GetUsersAsync())
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Name = u.UserName,
                    Email = u.Email,
                    IsRegisteredInSystem = u.IsRegisteredInSystem,
                    IsLoggedInViaFacebook = u.LoginProviders.Contains(ExternalProviderNames.Facebook),
                    IsLoggedInViaGoogle = u.LoginProviders.Contains(ExternalProviderNames.Google),
                    IsLoggedInViaGithub = u.LoginProviders.Contains(ExternalProviderNames.GitHub),
                    IsEmailConfirmed = u.IsEmailConfirmed,
                    IsAdmin = u.Roles.Contains("admin")
                })
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int? id)
        {
            if (id.HasValue)
            {
                await userService.DeleteUserByIdAsync(id.Value);
            }

            return RedirectToPage();
        }

        public class UserViewModel
        {
            public required int Id { get; init; }

            public required string? Name { get; init; }

            public required string? Email { get; init; }

            public required bool IsRegisteredInSystem { get; init; }

            public required bool IsLoggedInViaFacebook { get; init; }

            public required bool IsLoggedInViaGoogle { get; init; }

            public required bool IsLoggedInViaGithub { get; init; }

            public required bool IsEmailConfirmed { get; init; }

            public required bool IsAdmin { get; init; }
        }
    }
}
