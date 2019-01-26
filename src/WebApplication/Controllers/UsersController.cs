using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models;
using Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Controllers
{
  [ApiController]
  [Route("/api/v1/[controller]")]
  public class UsersController : Controller
  {
    private readonly UserManager<User> userManager;
    private readonly JwtOptions jwtOptions;

    public UsersController(UserManager<User> userManager,
                           IOptions<JwtOptions> jwtOptions)
    {
      this.userManager = userManager;
      this.jwtOptions = jwtOptions.Value;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAsync(UserCreateApiModel userCreateApiModel)
    {
      User user = new User { UserName = userCreateApiModel.Name, Email = userCreateApiModel.Email };

      IdentityResult identityCreateResult = await userManager.CreateAsync(user, userCreateApiModel.Password);

      if (identityCreateResult.Succeeded)
      {
        string userRole = "user";

        await userManager.AddToRoleAsync(user, userRole);

        return Json(GenerateToken(user, new List<string> { userRole }));
      }
      else
      {
        return BadRequest(new { errors = identityCreateResult.Errors.Select(e => e.Description).ToList() });
      }
    }

    private async Task<string> GenerateTokenAsync(User user)
    {
      return GenerateToken(user, await userManager.GetRolesAsync(user));
    }

    private string GenerateToken(User user, IList<string> userRoles)
    {
      IList<Claim> userClaims = userRoles
        .Select(r => new Claim(ClaimTypes.Role, r))
        .ToList();

      userClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

      return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
        claims: userClaims,
        expires: DateTime.UtcNow.AddMonths(1),
        signingCredentials: new SigningCredentials(jwtOptions.SecurityKey, SecurityAlgorithms.HmacSha256)
      ));
    }
  }
}
