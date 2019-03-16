using System.Threading.Tasks;

namespace Services
{
  public interface IUserRoleService
  {
    Task CreateAsync(int userId, string roleName);
  }
}
