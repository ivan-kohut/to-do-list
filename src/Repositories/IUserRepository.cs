using Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
  public interface IUserRepository
  {
    Task<User?> GetByIdAsync(int id);
    IQueryable<User> GetAll();
    void Delete(User user);
  }
}
