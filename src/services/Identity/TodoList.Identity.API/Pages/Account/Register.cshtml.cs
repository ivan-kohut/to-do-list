using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;
using TodoList.Identity.API.Events;
using TodoList.Identity.API.Services;

namespace TodoList.Identity.API.Pages.Account
{
    public class RegisterPageModel : PageModel
    {
        private readonly UserManager<User> userManager;
        private readonly IEmailService emailService;
        private readonly IEventBusService eventBusService;

        public RegisterPageModel(
            UserManager<User> userManager,
            IEmailService emailService,
            IEventBusService eventBusService)
        {
            this.userManager = userManager;
            this.emailService = emailService;
            this.eventBusService = eventBusService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public async Task<IActionResult> OnPostAsync(string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                User? user = await userManager.FindByEmailAsync(Input.Email!);

                if (user != null && user.PasswordHash != null)
                {
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Email)}", $"The User with the email already exists");

                    return Page();
                }
                else if (user == null)
                {
                    user = new User { UserName = Input.Name, Email = Input.Email };

                    IdentityResult identityCreateResult = await userManager.CreateAsync(user, Input.Password!);

                    if (!identityCreateResult.Succeeded)
                    {
                        AddModelErrors(identityCreateResult);

                        return Page();
                    }

                    await userManager.AddToRoleAsync(user, "user");

                    eventBusService.Publish(new UserCreatedIntegrationEvent(user.Id));
                }
                else
                {
                    user.UserName = Input.Name;

                    foreach (IUserValidator<User> userValidator in userManager.UserValidators)
                    {
                        IdentityResult identityValidateResult = await userValidator.ValidateAsync(userManager, user);

                        if (!identityValidateResult.Succeeded)
                        {
                            AddModelErrors(identityValidateResult);

                            return Page();
                        }
                    }

                    IdentityResult identityUpdateResult = await userManager.AddPasswordAsync(user, Input.Password!);

                    if (!identityUpdateResult.Succeeded)
                    {
                        AddModelErrors(identityUpdateResult);

                        return Page();
                    }
                }

                await emailService.SendEmailAsync(user.Email!, "Confirm your email", await GenerateEmailConfirmationMessageAsync(user, returnUrl));

                // TO DO
                return RedirectToPage("RegisterSuccess");
            }

            return Page();
        }

        private void AddModelErrors(IdentityResult identityResult)
        {
            foreach (IdentityError error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task<string> GenerateEmailConfirmationMessageAsync(User user, string? returnUrl)
        {
            // TO DO
            string callbackUrl = Url.Page(
                "ConfirmEmail",
                pageHandler: null,
                values: new { id = user.Id, code = await userManager.GenerateEmailConfirmationTokenAsync(user), returnUrl },
                protocol: Request.Scheme)!;

            return $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";
        }

        public class InputModel
        {
            [Required]
            public string? Name { get; set; }

            [Required]
            [EmailAddress]
            public string? Email { get; set; }

            [Required]
            public string? Password { get; set; }

            [Required]
            [Compare(nameof(Password))]
            public string? ConfirmPassword { get; set; }
        }
    }
}
