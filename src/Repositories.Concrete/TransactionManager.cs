using System.Threading.Tasks;

namespace Repositories
{
  public class TransactionManager : ITransactionManager
  {
    private readonly AppDbContext dbContext;

    public TransactionManager(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public async Task SaveChangesAsync()
    {
      await dbContext.SaveChangesAsync();
    }
  }
}
