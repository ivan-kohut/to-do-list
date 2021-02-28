using Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Repositories
{
  public class UserRepository : IUserRepository
  {
    private readonly AppDbContext dbContext;

    public UserRepository(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public async Task<User?> GetUserAsync(int identityId) =>
      await dbContext
        .Users
        .SingleOrDefaultAsync(u => u.IdentityId == identityId);

    public void Create(User user) => dbContext.Add(user);
  }
}
