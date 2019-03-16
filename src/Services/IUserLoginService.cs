using System.Threading.Tasks;

namespace Services
{
  public interface IUserLoginService
  {
    Task CreateAsync(int userId, string loginProvider, string providerKey);
    Task<bool> ExistsWithUserIdAndLoginProviderAsync(int userId, string loginProvider);
  }
}
