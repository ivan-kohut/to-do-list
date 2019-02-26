using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
  public interface IUserService
  {
    Task<IEnumerable<UserDTO>> GetAllAsync();
    Task DeleteAsync(int id);
  }
}
