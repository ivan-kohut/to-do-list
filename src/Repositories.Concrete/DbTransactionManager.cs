using System.Threading.Tasks;

namespace Repositories
{
  public class DbTransactionManager : IDbTransactionManager
  {
    private readonly AppDbContext dbContext;

    public DbTransactionManager(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public async Task SaveChangesAsync()
    {
      await dbContext.SaveChangesAsync();
    }
  }
}
