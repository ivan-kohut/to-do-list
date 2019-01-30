using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models;
using Options;
using Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Controllers
{
  [ApiController]
  [Route("/api/v1/[controller]")]
  public class UsersController : Controller
  {
    private readonly IEmailService emailService;
    private readonly UserManager<User> userManager;
    private readonly JwtOptions jwtOptions;

    public UsersController(IEmailService emailService,
                           UserManager<User> userManager,
                           IOptions<JwtOptions> jwtOptions)
    {
      this.emailService = emailService;
      this.userManager = userManager;
      this.jwtOptions = jwtOptions.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(UserLoginModel userLoginModel)
    {
      IActionResult actionResult;

      User user = await userManager.FindByEmailAsync(userLoginModel.Email);

      if (user == null)
      {
        actionResult = NotFound(new { errors = new[] { $"User with email '{userLoginModel.Email}' is not found" } });
      }
      else if (await userManager.CheckPasswordAsync(user, userLoginModel.Password))
      {
        if (!user.EmailConfirmed)
        {
          actionResult = BadRequest(new { errors = new[] { "Email is not confirmed. Please, go to your email account" } });
        }
        else
        {
          actionResult = Json(await GenerateTokenAsync(user));
        }
      }
      else
      {
        actionResult = BadRequest(new { errors = new[] { "User password is not valid" } });
      }

      return actionResult;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAsync(UserCreateModel userCreateModel)
    {
      if (await userManager.FindByEmailAsync(userCreateModel.Email) != null)
      {
        return BadRequest(new { errors = new[] { $"User with email '{userCreateModel.Email}' exists already" } });
      }

      User user = new User { UserName = userCreateModel.Name, Email = userCreateModel.Email };

      IdentityResult identityCreateResult = await userManager.CreateAsync(user, userCreateModel.Password);

      if (identityCreateResult.Succeeded)
      {
        await userManager.AddToRoleAsync(user, "user");

        string emailConfirmationMessage = await GenerateEmailConfirmationMessageAsync(user);

        await emailService.SendEmailAsync(user.Email, "Confirm your email", emailConfirmationMessage);

        return Ok();
      }
      else
      {
        return BadRequest(new { errors = identityCreateResult.Errors.Select(e => e.Description).ToList() });
      }
    }

    [HttpGet("{id}/email-confirmation")]
    public async Task<IActionResult> ConfirmEmailAsync(int id, string code)
    {
      if (string.IsNullOrWhiteSpace(code))
      {
        return BadRequest();
      }

      User user = await userManager.FindByIdAsync(id.ToString());

      if (user == null || user.EmailConfirmed)
      {
        return BadRequest();
      }
      else if ((await userManager.ConfirmEmailAsync(user, code)).Succeeded)
      {
        return Ok("Your email is confirmed");
      }
      else
      {
        return BadRequest();
      }
    }

    private async Task<string> GenerateTokenAsync(User user)
    {
      IList<Claim> userClaims = (await userManager.GetRolesAsync(user))
        .Select(r => new Claim(ClaimTypes.Role, r))
        .ToList();

      userClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

      return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
        claims: userClaims,
        expires: DateTime.UtcNow.AddMonths(1),
        signingCredentials: new SigningCredentials(jwtOptions.SecurityKey, SecurityAlgorithms.HmacSha256)
      ));
    }

    private async Task<string> GenerateEmailConfirmationMessageAsync(User user)
    {
      string callbackUrl = Url.Action(
        nameof(ConfirmEmailAsync),
        "Users",
        new { id = user.Id, code = await userManager.GenerateEmailConfirmationTokenAsync(user) },
        protocol: HttpContext.Request.Scheme
      );

      return $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";
    }
  }
}
