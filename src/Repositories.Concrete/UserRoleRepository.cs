using Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Repositories
{
  public class UserRoleRepository : IUserRoleRepository
  {
    private readonly AppDbContext dbContext;

    public UserRoleRepository(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public async Task<UserRole?> GetByUserIdAndRoleIdAsync(int userId, int roleId)
    {
      return await dbContext
        .UserRoles
        .SingleOrDefaultAsync(r => r.UserId == userId && r.RoleId == roleId);
    }

    public async Task CreateAsync(UserRole userRole)
    {
      await dbContext.AddAsync(userRole);
    }
  }
}
