namespace Services
{
  public interface IJwtTokenService
  {
    string GenerateToken(int userId);
  }
}
