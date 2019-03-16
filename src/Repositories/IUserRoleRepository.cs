using Entities;
using System.Threading.Tasks;

namespace Repositories
{
  public interface IUserRoleRepository
  {
    Task<UserRole> GetByUserIdAndRoleIdAsync(int userId, int roleId);
    Task CreateAsync(UserRole userRole);
  }
}
