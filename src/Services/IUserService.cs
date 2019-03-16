using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
  public interface IUserService
  {
    Task<UserDTO> GetByIdAsync(int id);
    Task<IEnumerable<UserDTO>> GetAllAsync();
    Task DeleteAsync(int id);
  }
}
