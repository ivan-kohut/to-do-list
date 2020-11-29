using Entities;
using System.Threading.Tasks;

namespace Repositories
{
  public interface IUserRepository
  {
    Task<User?> GetUserAsync(int identityId);
  }
}
