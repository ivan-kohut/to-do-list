using Microsoft.IdentityModel.Tokens;

namespace Options
{
  public class JwtOptions
  {
    public SecurityKey SecurityKey { get; set; } = null!;
  }
}
