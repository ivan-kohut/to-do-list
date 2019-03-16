using Entities;
using System.Threading.Tasks;

namespace Repositories
{
  public interface IRoleRepository
  {
    Task<Role> GetByNameAsync(string name);
  }
}
