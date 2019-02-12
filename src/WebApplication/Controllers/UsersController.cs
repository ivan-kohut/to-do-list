﻿using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models;
using Newtonsoft.Json;
using Options;
using Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Controllers
{
  [ApiController]
  [Route("/api/v1/[controller]")]
  public class UsersController : Controller
  {
    private const string symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private readonly IEmailService emailService;
    private readonly UserManager<User> userManager;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly JwtOptions jwtOptions;
    private readonly FacebookOptions facebookOptions;

    public UsersController(IEmailService emailService,
                           UserManager<User> userManager,
                           IHttpClientFactory httpClientFactory,
                           IOptions<JwtOptions> jwtOptions,
                           IOptions<FacebookOptions> facebookOptions)
    {
      this.emailService = emailService;
      this.userManager = userManager;
      this.httpClientFactory = httpClientFactory;
      this.jwtOptions = jwtOptions.Value;
      this.facebookOptions = facebookOptions.Value;
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
          if (user.TwoFactorEnabled)
          {
            if (string.IsNullOrWhiteSpace(userLoginModel.TwoFactorToken))
            {
              actionResult = StatusCode(427, new { errors = new[] { "Enter authentication code" } });
            }
            else
            {
              bool isTwoFactorTokenValid = await userManager.VerifyTwoFactorTokenAsync(
                user,
                userManager.Options.Tokens.AuthenticatorTokenProvider,
                userLoginModel.TwoFactorToken
              );

              if (isTwoFactorTokenValid)
              {
                actionResult = Json(await GenerateTokenAsync(user));
              }
              else
              {
                actionResult = BadRequest(new { errors = new[] { "Verification code is invalid." } });
              }
            }
          }
          else
          {
            actionResult = Json(await GenerateTokenAsync(user));
          }
        }
      }
      else
      {
        actionResult = BadRequest(new { errors = new[] { "User password is not valid" } });
      }

      return actionResult;
    }

    [HttpPost("facebook-login")]
    public async Task<IActionResult> LoginByFacebookAsync(UserFacebookLoginModel userFacebookLoginModel)
    {
      HttpClient httpClient = httpClientFactory.CreateClient();

      HttpResponseMessage accessTokenResponse = await httpClient
        .GetAsync($"{facebookOptions.GraphApiEndpoint}/oauth/access_token?client_id={facebookOptions.AppId}&client_secret={facebookOptions.AppSecret}&redirect_uri={userFacebookLoginModel.RedirectUri}&code={userFacebookLoginModel.Code}");

      if (!accessTokenResponse.IsSuccessStatusCode)
      {
        return BadRequest();
      }

      UserFacebookAccessToken userFacebookAccessToken = JsonConvert.DeserializeObject<UserFacebookAccessToken>(await accessTokenResponse.Content.ReadAsStringAsync());

      string userInfoResponse = await httpClient
        .GetStringAsync($"{facebookOptions.GraphApiEndpoint}/me?fields=email&access_token={userFacebookAccessToken.AccessToken}");

      UserFacebookData userFacebookData = JsonConvert.DeserializeObject<UserFacebookData>(userInfoResponse);

      User user = await userManager.FindByEmailAsync(userFacebookData.Email);

      if (user == null)
      {
        user = new User { UserName = userFacebookData.Email, Email = userFacebookData.Email };

        await userManager.CreateAsync(user);
        await userManager.AddToRoleAsync(user, "user");
        await AddLoginAsync(user, "Facebook", userFacebookData.Id.ToString());
      }
      else if (!(await userManager.GetLoginsAsync(user)).Any(l => l.LoginProvider == "Facebook"))
      {
        await AddLoginAsync(user, "Facebook", userFacebookData.Id.ToString());
      }

      return Json(await GenerateTokenAsync(user));
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
        return BadRequest(new { errors = GenerateErrorMessages(identityCreateResult) });
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

    [HttpPost("password-recovery")]
    public async Task<IActionResult> RecoverPassword(UserForgotPasswordModel userForgotPasswordModel)
    {
      User user = await userManager.FindByEmailAsync(userForgotPasswordModel.Email);

      if (user == null)
      {
        return NotFound(new { errors = new[] { $"User with email '{userForgotPasswordModel.Email}' is not found" } });
      }

      string randomPassword = GenerateRandomPassword();

      user.PasswordHash = userManager.PasswordHasher.HashPassword(user, randomPassword);

      await userManager.UpdateAsync(user);
      await emailService.SendEmailAsync(user.Email, "Password recovery", GeneratePasswordRecoveryMessage(randomPassword));

      return Ok();
    }

    [Authorize(Roles = "user")]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(UserChangePasswordModel userChangePasswordModel)
    {
      User user = await userManager.GetUserAsync(User);

      IdentityResult identityChangePasswordResult = await userManager.ChangePasswordAsync(
        user, userChangePasswordModel.OldPassword, userChangePasswordModel.NewPassword
      );

      if (identityChangePasswordResult.Succeeded)
      {
        return Ok();
      }
      else
      {
        return BadRequest(new { errors = GenerateErrorMessages(identityChangePasswordResult) });
      }
    }

    [Authorize(Roles = "user")]
    [HttpGet("authenticator-uri")]
    public async Task<ActionResult<string>> GetAuthenticatorUri()
    {
      User user = await userManager.GetUserAsync(User);

      string authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
      string authenticatorUri = null;

      if (!string.IsNullOrWhiteSpace(authenticatorKey))
      {
        return $"otpauth://totp/ToDoList:{user.UserName}?secret={authenticatorKey}&issuer=ToDoList&digits=6";
      }

      return authenticatorUri;
    }

    [Authorize(Roles = "user")]
    [HttpPut("authenticator-key")]
    public async Task<IActionResult> ResetAuthenticatorKey()
    {
      await userManager.ResetAuthenticatorKeyAsync(await userManager.GetUserAsync(User));

      return Ok();
    }

    [Authorize(Roles = "user")]
    [HttpPut("enable-authenticator")]
    public async Task<IActionResult> EnableAuthenticator(UserEnableAuthenticatorModel userEnableAuthenticatorModel)
    {
      User user = await userManager.GetUserAsync(User);

      if (user.TwoFactorEnabled)
      {
        return BadRequest();
      }

      bool isTwoFactorTokenValid = await userManager.VerifyTwoFactorTokenAsync(
        user,
        userManager.Options.Tokens.AuthenticatorTokenProvider,
        userEnableAuthenticatorModel.Code
      );

      if (isTwoFactorTokenValid)
      {
        await userManager.SetTwoFactorEnabledAsync(user, true);

        return Ok();
      }
      else
      {
        return BadRequest(new { errors = new[] { "Verification code is invalid." } });
      }
    }

    [Authorize(Roles = "user")]
    [HttpPut("disable-two-factor-authentication")]
    public async Task<IActionResult> DisableTwoFactorAuthentication()
    {
      User user = await userManager.GetUserAsync(User);

      if (!user.TwoFactorEnabled)
      {
        return BadRequest();
      }

      await userManager.SetTwoFactorEnabledAsync(user, false);
      await userManager.ResetAuthenticatorKeyAsync(user);

      return Ok();
    }

    [Authorize(Roles = "user")]
    [HttpGet("two-factor-authentication-enabled")]
    public async Task<ActionResult<bool>> IsTwoFactorAuthenticationEnabled()
    {
      return (await userManager.GetUserAsync(User)).TwoFactorEnabled;
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

    private string GeneratePasswordRecoveryMessage(string password)
    {
      return $"The '{password}' is your new random password for login. Please change it in your account for security.";
    }

    private string GenerateRandomPassword()
    {
      Random random = new Random();

      return new string(
        Enumerable
          .Repeat(symbols, 10)
          .Select(s => s[random.Next(s.Length)])
          .ToArray()
      );
    }

    private IEnumerable<string> GenerateErrorMessages(IdentityResult identityResult)
    {
      return identityResult
        .Errors
        .Select(e => e.Description)
        .ToList();
    }

    private async Task AddLoginAsync(User user, string loginProvider, string providerKey)
    {
      await userManager.AddLoginAsync(user, new UserLoginInfo(loginProvider, providerKey, loginProvider));
    }
  }
}
