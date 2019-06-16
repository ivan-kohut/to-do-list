using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using API.Models;
using Newtonsoft.Json;
using Options;
using Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Controllers
{
  [ApiController]
  [Produces("application/json")]
  [Route(Urls.Users)]
  public class UsersController : Controller
  {
    private const string symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private readonly IUserService userService;
    private readonly IUserRoleService userRoleService;
    private readonly IUserLoginService userLoginService;
    private readonly IItemService itemService;
    private readonly IEmailService emailService;
    private readonly UserManager<User> userManager;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly JwtOptions jwtOptions;
    private readonly FacebookOptions facebookOptions;
    private readonly GoogleOptions googleOptions;
    private readonly GithubOptions githubOptions;
    private readonly LinkedInOptions linkedInOptions;

    public UsersController(IUserService userService,
                           IUserRoleService userRoleService,
                           IUserLoginService userLoginService,
                           IItemService itemService,
                           IEmailService emailService,
                           UserManager<User> userManager,
                           IHttpClientFactory httpClientFactory,
                           IOptions<JwtOptions> jwtOptions,
                           IOptions<FacebookOptions> facebookOptions,
                           IOptions<GoogleOptions> googleOptions,
                           IOptions<GithubOptions> githubOptions,
                           IOptions<LinkedInOptions> linkedInOptions)
    {
      this.userService = userService;
      this.userRoleService = userRoleService;
      this.userLoginService = userLoginService;
      this.itemService = itemService;
      this.emailService = emailService;
      this.userManager = userManager;
      this.httpClientFactory = httpClientFactory;
      this.jwtOptions = jwtOptions.Value;
      this.facebookOptions = facebookOptions.Value;
      this.googleOptions = googleOptions.Value;
      this.githubOptions = githubOptions.Value;
      this.linkedInOptions = linkedInOptions.Value;
    }

    /// <response code="401">If user does not have role "admin"</response>
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<UserListApiModel>>> GetAllAsync()
    {
      return (await userService.GetAllAsync())
        .Select(u => new UserListApiModel
        {
          Id = u.Id,
          Name = u.Name,
          Email = u.Email,
          IsEmailConfirmed = u.IsEmailConfirmed
        })
        .ToList();
    }

    /// <response code="401">If user does not have role "admin"</response> 
    /// <response code="404">If user is not found by id</response> 
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
      await userService.DeleteAsync(id);

      return Ok();
    }

    /// <response code="400">Email is not confirmed or two-factor code is invalid or password is not valid</response> 
    /// <response code="427">If two-factor code is missing</response> 
    /// <response code="404">If user is not found by email</response> 
    [HttpPost(Urls.Login)]
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

    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [HttpPost(Urls.LoginByFacebook)]
    public async Task<IActionResult> LoginByFacebookAsync(UserExternalLoginModel userExternalLoginModel)
    {
      HttpClient httpClient = httpClientFactory.CreateClient();

      HttpResponseMessage accessTokenResponse = await httpClient
        .GetAsync($"{facebookOptions.GraphApiEndpoint}/oauth/access_token?client_id={facebookOptions.AppId}&client_secret={facebookOptions.AppSecret}&redirect_uri={userExternalLoginModel.RedirectUri}&code={userExternalLoginModel.Code}");

      if (!accessTokenResponse.IsSuccessStatusCode)
      {
        return BadRequest();
      }

      UserExternalLoginAccessToken userExternalLoginAccessToken = JsonConvert.DeserializeObject<UserExternalLoginAccessToken>(await accessTokenResponse.Content.ReadAsStringAsync());

      string userInfoResponse = await httpClient
        .GetStringAsync($"{facebookOptions.GraphApiEndpoint}/me?fields=email&access_token={userExternalLoginAccessToken.AccessToken}");

      return Json(await GenerateTokenAsync("Facebook", userInfoResponse));
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [HttpPost(Urls.LoginByGoogle)]
    public async Task<IActionResult> LoginByGoogleAsync(UserExternalLoginModel userExternalLoginModel)
    {
      HttpClient httpClient = httpClientFactory.CreateClient();

      object requestBody = new
      {
        grant_type = "authorization_code",
        code = userExternalLoginModel.Code,
        client_id = googleOptions.ClientId,
        client_secret = googleOptions.ClientSecret,
        redirect_uri = userExternalLoginModel.RedirectUri
      };

      using (HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"))
      {
        HttpResponseMessage accessTokenResponse = await httpClient.PostAsync(googleOptions.AccessTokenEndpoint, httpContent);

        if (!accessTokenResponse.IsSuccessStatusCode)
        {
          return BadRequest();
        }

        UserExternalLoginAccessToken userExternalLoginAccessToken = JsonConvert.DeserializeObject<UserExternalLoginAccessToken>(await accessTokenResponse.Content.ReadAsStringAsync());

        string userInfoResponse = await httpClient
          .GetStringAsync($"{googleOptions.UserInfoEndpoint}?access_token={userExternalLoginAccessToken.AccessToken}");

        return Json(await GenerateTokenAsync("Google", userInfoResponse));
      }
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [HttpPost(Urls.LoginByGithub)]
    public async Task<IActionResult> LoginByGithubAsync(UserExternalLoginModel userExternalLoginModel)
    {
      HttpClient httpClient = httpClientFactory.CreateClient();

      object requestBody = new
      {
        code = userExternalLoginModel.Code,
        client_id = githubOptions.ClientId,
        client_secret = githubOptions.ClientSecret
      };

      using (HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"))
      {
        HttpResponseMessage accessTokenResponse = await httpClient.PostAsync(githubOptions.AccessTokenEndpoint, httpContent);

        if (!accessTokenResponse.IsSuccessStatusCode)
        {
          return BadRequest();
        }

        string accessToken = (await accessTokenResponse.Content.ReadAsStringAsync())
          .Split("&")
          .Where(p => p.StartsWith("access_token"))
          .Select(p => p.Split("=").Last())
          .Single();

        httpClient.DefaultRequestHeaders.Add("User-Agent", "Todo List API");

        string userInfoResponse = await httpClient
          .GetStringAsync($"{githubOptions.UserInfoEndpoint}?access_token={accessToken}");

        return Json(await GenerateTokenAsync("Github", userInfoResponse));
      }
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [HttpPost(Urls.LoginByLinkedin)]
    public async Task<IActionResult> LoginByLinkedInAsync(UserExternalLoginModel userExternalLoginModel)
    {
      HttpClient httpClient = httpClientFactory.CreateClient();

      object requestBody = new
      {
        grant_type = "authorization_code",
        code = userExternalLoginModel.Code,
        client_id = linkedInOptions.ClientId,
        client_secret = linkedInOptions.ClientSecret,
        redirect_uri = userExternalLoginModel.RedirectUri
      };

      using (HttpContent httpContent = new FormUrlEncodedContent(JsonConvert.DeserializeObject<IDictionary<string, string>>(JsonConvert.SerializeObject(requestBody))))
      {
        HttpResponseMessage accessTokenResponse = await httpClient.PostAsync(linkedInOptions.AccessTokenEndpoint, httpContent);

        if (!accessTokenResponse.IsSuccessStatusCode)
        {
          return BadRequest();
        }

        UserExternalLoginAccessToken userExternalLoginAccessToken = JsonConvert.DeserializeObject<UserExternalLoginAccessToken>(await accessTokenResponse.Content.ReadAsStringAsync());

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userExternalLoginAccessToken.AccessToken);

        LinkedInUserIdModel linkedInUserIdModel = JsonConvert.DeserializeObject<LinkedInUserIdModel>(await httpClient.GetStringAsync(linkedInOptions.IdEndpoint));

        string userEmail = JsonConvert.DeserializeObject<LinkedInUserEmailModel>(await httpClient.GetStringAsync(linkedInOptions.EmailAddressEndpoint))
          .Elements
          .Single()
          .Handle
          .EmailAddress;

        return Json(await GenerateTokenAsync("LinkedIn", linkedInUserIdModel.Id, userEmail));
      }
    }

    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateUserAsync(UserCreateModel userCreateModel)
    {
      User user = await userManager.FindByEmailAsync(userCreateModel.Email);

      if (user != null && user.PasswordHash != null)
      {
        return BadRequest(new { errors = new[] { $"User with email '{userCreateModel.Email}' exists already" } });
      }
      else if (user == null)
      {
        user = new User { UserName = userCreateModel.Name, Email = userCreateModel.Email };

        IdentityResult identityCreateResult = await userManager.CreateAsync(user, userCreateModel.Password);

        if (identityCreateResult.Succeeded)
        {
          await userRoleService.CreateAsync(user.Id, "user");
        }
        else
        {
          return BadRequest(new { errors = GenerateErrorMessages(identityCreateResult) });
        }
      }
      else
      {
        user.UserName = userCreateModel.Name;

        foreach (IUserValidator<User> userValidator in userManager.UserValidators)
        {
          IdentityResult identityValidateResult = await userValidator.ValidateAsync(userManager, user);

          if (!identityValidateResult.Succeeded)
          {
            return BadRequest(new { errors = GenerateErrorMessages(identityValidateResult) });
          }
        }

        IdentityResult identityUpdateResult = await userManager.AddPasswordAsync(user, userCreateModel.Password);

        if (!identityUpdateResult.Succeeded)
        {
          return BadRequest(new { errors = GenerateErrorMessages(identityUpdateResult) });
        }
      }

      await emailService.SendEmailAsync(user.Email, "Confirm your email", await GenerateEmailConfirmationMessageAsync(user));

      return Ok();
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [HttpGet("{id}/email/confirm")]
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

    /// <response code="404">If user is not found by email</response> 
    [HttpPost(Urls.PasswordRecovery)]
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

    /// <response code="401">If user does not have role "user"</response> 
    [Authorize(Roles = "admin,user")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [HttpPut(Urls.ChangePassword)]
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

    ///// <response code="401">If user does not have role "user"</response> 
    [Authorize(Roles = "admin,user")]
    [HttpGet(Urls.AuthenticatorUri)]
    public async Task<ActionResult<string>> GetAuthenticatorUri()
    {
      User user = await userManager.GetUserAsync(User);

      string authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);

      if (string.IsNullOrWhiteSpace(authenticatorKey))
      {
        await userManager.ResetAuthenticatorKeyAsync(user);

        authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
      }

      return $"otpauth://totp/ToDoList:{user.UserName}?secret={authenticatorKey}&issuer=ToDoList&digits=6";
    }

    /// <response code="400">Two factor is already enabled or verification code is invalid</response>
    /// <response code="401">If user does not have role "user"</response> 
    [Authorize(Roles = "admin,user")]
    [HttpPut(Urls.EnableTwoFactorAuthentication)]
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

    /// <response code="400">Two factor is disabled already</response>
    /// <response code="401">If user does not have role "user"</response> 
    [Authorize(Roles = "admin,user")]
    [HttpPut(Urls.DisableTwoFactorAuthentication)]
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

    /// <response code="401">If user does not have role "user"</response>
    [Authorize(Roles = "admin,user")]
    [HttpGet(Urls.IsTwoFactorAuthenticationEnabled)]
    public async Task<ActionResult<bool>> IsTwoFactorAuthenticationEnabled()
    {
      return (await userManager.GetUserAsync(User)).TwoFactorEnabled;
    }

    private async Task<string> GenerateTokenAsync(string loginProvider, string userInfoResponse)
    {
      UserExternalLoginData userExternalLoginData = JsonConvert.DeserializeObject<UserExternalLoginData>(userInfoResponse);

      return await GenerateTokenAsync(loginProvider, userExternalLoginData.Id, userExternalLoginData.Email);
    }

    private async Task<string> GenerateTokenAsync(string loginProvider, string id, string email)
    {
      User user = await userManager.FindByEmailAsync(email);

      if (user == null)
      {
        user = new User { UserName = email, Email = email };

        await userManager.CreateAsync(user);
        await userRoleService.CreateAsync(user.Id, "user");
        await userLoginService.CreateAsync(user.Id, loginProvider, id);
      }
      else if (!await userLoginService.ExistsWithUserIdAndLoginProviderAsync(user.Id, loginProvider))
      {
        await userLoginService.CreateAsync(user.Id, loginProvider, id);
      }

      return await GenerateTokenAsync(user);
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
  }
}
