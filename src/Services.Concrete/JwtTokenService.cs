using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Services
{
  public class JwtTokenService : IJwtTokenService
  {
    private readonly JwtOptions jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
      jwtOptions = options.Value;
    }

    public string GenerateToken(int userId)
    {
      return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
        claims: Enumerable.Empty<Claim>(), // set user claims
        expires: DateTime.UtcNow.AddMonths(1),
        signingCredentials: new SigningCredentials(jwtOptions.SecurityKey, SecurityAlgorithms.HmacSha256)
      ));
    }
  }
}
