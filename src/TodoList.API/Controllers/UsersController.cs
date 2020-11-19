using API.Models;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
using System.Threading.Tasks;

namespace Controllers
{
  [ApiController]
  [Produces("application/json")]
  [ProducesResponseType(200)]
  [Route(Urls.Users)]
  public class UsersController : Controller
  {
    private const string symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private readonly IUserRoleService userRoleService;
    private readonly IUserLoginService userLoginService;
    private readonly UserManager<User> userManager;
    private readonly JwtOptions jwtOptions;

    public UsersController(IUserRoleService userRoleService,
                           IUserLoginService userLoginService,
                           UserManager<User> userManager,
                           IOptions<JwtOptions> jwtOptions)
    {
      this.userRoleService = userRoleService;
      this.userLoginService = userLoginService;
      this.userManager = userManager;
      this.jwtOptions = jwtOptions.Value;
    }

    [ProducesResponseType(400)]
    [HttpPost(Urls.LoginByFacebook)]
    public async Task<IActionResult> LoginByFacebookAsync(
      UserExternalLoginModel userExternalLoginModel,
      [FromServices] IHttpClientFactory httpClientFactory,
      [FromServices] IOptions<FacebookOptions> facebookIOptions)
    {
      HttpClient httpClient = httpClientFactory.CreateClient();
      FacebookOptions facebookOptions = facebookIOptions.Value;

      HttpResponseMessage accessTokenResponse = await httpClient
        .GetAsync($"{facebookOptions.GraphApiEndpoint}/oauth/access_token?client_id={facebookOptions.AppId}&client_secret={facebookOptions.AppSecret}&redirect_uri={userExternalLoginModel.RedirectUri}&code={userExternalLoginModel.Code}");

      if (!accessTokenResponse.IsSuccessStatusCode || accessTokenResponse.Content == null)
      {
        return BadRequest();
      }

      UserExternalLoginAccessToken userExternalLoginAccessToken = JsonConvert.DeserializeObject<UserExternalLoginAccessToken>(await accessTokenResponse.Content.ReadAsStringAsync());

      string userInfoResponse = await httpClient
        .GetStringAsync($"{facebookOptions.GraphApiEndpoint}/me?fields=email&access_token={userExternalLoginAccessToken.AccessToken}");

      return Json(await GenerateTokenAsync("Facebook", userInfoResponse));
    }

    [ProducesResponseType(400)]
    [HttpPost(Urls.LoginByGoogle)]
    public async Task<IActionResult> LoginByGoogleAsync(
      UserExternalLoginModel userExternalLoginModel,
      [FromServices] IHttpClientFactory httpClientFactory,
      [FromServices] IOptions<GoogleOptions> googleIOptions)
    {
      HttpClient httpClient = httpClientFactory.CreateClient();
      GoogleOptions googleOptions = googleIOptions.Value;

      object requestBody = new
      {
        grant_type = "authorization_code",
        code = userExternalLoginModel.Code,
        client_id = googleOptions.ClientId,
        client_secret = googleOptions.ClientSecret,
        redirect_uri = userExternalLoginModel.RedirectUri
      };

      using HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

      HttpResponseMessage accessTokenResponse = await httpClient.PostAsync(googleOptions.AccessTokenEndpoint, httpContent);

      if (!accessTokenResponse.IsSuccessStatusCode || accessTokenResponse.Content == null)
      {
        return BadRequest();
      }

      UserExternalLoginAccessToken userExternalLoginAccessToken = JsonConvert.DeserializeObject<UserExternalLoginAccessToken>(await accessTokenResponse.Content.ReadAsStringAsync());

      string userInfoResponse = await httpClient
        .GetStringAsync($"{googleOptions.UserInfoEndpoint}?access_token={userExternalLoginAccessToken.AccessToken}");

      return Json(await GenerateTokenAsync("Google", userInfoResponse));
    }

    [ProducesResponseType(400)]
    [HttpPost(Urls.LoginByGithub)]
    public async Task<IActionResult> LoginByGithubAsync(
      UserExternalLoginModel userExternalLoginModel,
      [FromServices] IHttpClientFactory httpClientFactory,
      [FromServices] IOptions<GithubOptions> githubIOptions)
    {
      HttpClient httpClient = httpClientFactory.CreateClient();
      GithubOptions githubOptions = githubIOptions.Value;

      object requestBody = new
      {
        code = userExternalLoginModel.Code,
        client_id = githubOptions.ClientId,
        client_secret = githubOptions.ClientSecret
      };

      using HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

      HttpResponseMessage accessTokenResponse = await httpClient.PostAsync(githubOptions.AccessTokenEndpoint, httpContent);

      if (!accessTokenResponse.IsSuccessStatusCode || accessTokenResponse.Content == null)
      {
        return BadRequest();
      }

      string accessToken = (await accessTokenResponse.Content.ReadAsStringAsync())
        .Split("&")
        .Where(p => p.StartsWith("access_token"))
        .Select(p => p.Split("=").Last())
        .Single();

      httpClient.DefaultRequestHeaders.Add("User-Agent", "Todo List API");
      httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

      string userInfoResponse = await httpClient.GetStringAsync(githubOptions.UserInfoEndpoint);

      return Json(await GenerateTokenAsync("Github", userInfoResponse));
    }

    [ProducesResponseType(400)]
    [HttpPost(Urls.LoginByLinkedin)]
    public async Task<IActionResult> LoginByLinkedInAsync(
      UserExternalLoginModel userExternalLoginModel,
      [FromServices] IHttpClientFactory httpClientFactory,
      [FromServices] IOptions<LinkedInOptions> linkedInIOptions)
    {
      HttpClient httpClient = httpClientFactory.CreateClient();
      LinkedInOptions linkedInOptions = linkedInIOptions.Value;

      object requestBody = new
      {
        grant_type = "authorization_code",
        code = userExternalLoginModel.Code,
        client_id = linkedInOptions.ClientId,
        client_secret = linkedInOptions.ClientSecret,
        redirect_uri = userExternalLoginModel.RedirectUri
      };

      using HttpContent httpContent = new FormUrlEncodedContent(JsonConvert.DeserializeObject<IDictionary<string?, string?>>(JsonConvert.SerializeObject(requestBody)));

      HttpResponseMessage accessTokenResponse = await httpClient.PostAsync(linkedInOptions.AccessTokenEndpoint, httpContent);

      if (!accessTokenResponse.IsSuccessStatusCode || accessTokenResponse.Content == null)
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

    /// <response code="404">If user is not found by email</response> 
    [HttpPost(Urls.PasswordRecovery)]
    public async Task<IActionResult> RecoverPassword(UserForgotPasswordModel userForgotPasswordModel, [FromServices] IEmailService emailService)
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

    /// <response code="403">If user does not have role "admin" or "user"</response> 
    [Authorize(Roles = "admin,user")]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
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
      userClaims.Add(new Claim(ClaimTypes.Name, user.UserName));

      return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
        claims: userClaims,
        expires: DateTime.UtcNow.AddMonths(1),
        signingCredentials: new SigningCredentials(jwtOptions.SecurityKey, SecurityAlgorithms.HmacSha256)
      ));
    }

    private string GeneratePasswordRecoveryMessage(string password) =>
      $"The '{password}' is your new random password for login. Please change it in your account for security.";

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

    private IEnumerable<string> GenerateErrorMessages(IdentityResult identityResult) =>
      identityResult
        .Errors
        .Select(e => e.Description)
        .ToList();
  }
}
