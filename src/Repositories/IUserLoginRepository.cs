using Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
  public interface IUserLoginRepository
  {
    IQueryable<UserLogin> GetAll();
    Task<UserLogin> GetByLoginProviderAndProviderKeyAsync(string loginProvider, string providerKey);
    Task CreateAsync(UserLogin userLogin);
  }
}
