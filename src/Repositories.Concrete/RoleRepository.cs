using Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Repositories
{
  public class RoleRepository : IRoleRepository
  {
    private readonly AppDbContext dbContext;

    public RoleRepository(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public async Task<Role> GetByNameAsync(string name)
    {
      return await dbContext
        .Roles
        .SingleOrDefaultAsync(r => r.NormalizedName == name.ToUpper());
    }
  }
}
