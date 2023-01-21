using System.Collections.Generic;
using System.Threading.Tasks;
using TodoList.Identity.API.Services.Models;

namespace TodoList.Identity.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetUsersAsync();

        Task DeleteUserByIdAsync(int id);
    }
}
