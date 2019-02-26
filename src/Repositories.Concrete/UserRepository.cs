using Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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

    public async Task<User> GetByIdAsync(int id)
    {
      return await dbContext
        .Users
        .Where(u => u.Id != 1)
        .SingleOrDefaultAsync(u => u.Id == id);
    }

    public IQueryable<User> GetAll()
    {
      return dbContext
        .Users
        .Where(u => u.Id != 1)
        .AsNoTracking();
    }

    public void Delete(User user)
    {
      dbContext.Remove(user);
    }
  }
}
