using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Identity.API.Data;
using TodoList.Identity.API.Data.Entities;
using TodoList.Identity.API.Services.Interfaces;
using TodoList.Identity.API.Services.Models;

namespace TodoList.Identity.API.Services
{
    public class UserService(AppDbContext dbContext) : IUserService
    {
        public async Task<IEnumerable<UserDTO>> GetUsersAsync()
        {
            return await dbContext.Users
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    IsRegisteredInSystem = u.PasswordHash != null,
                    IsEmailConfirmed = u.EmailConfirmed,
                    LoginProviders = u.UserLogins
                        !.Select(l => l.LoginProvider)
                        .ToList(),
                    Roles = u.UserRoles
                        !.Select(r => r.Role!.Name)
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task DeleteUserByIdAsync(int id)
        {
            User? userToDelete = await dbContext.Users
                .Where(u => u.Id == id && !u.UserRoles!.Any(r => r.Role!.Name == "admin"))
                .SingleOrDefaultAsync();

            if (userToDelete != default)
            {
                dbContext.Remove(userToDelete);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
