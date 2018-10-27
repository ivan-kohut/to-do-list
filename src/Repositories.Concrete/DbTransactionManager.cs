namespace Repositories
{
  public class DbTransactionManager : IDbTransactionManager
  {
    private readonly AppDbContext dbContext;

    public DbTransactionManager(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public void SaveChanges()
    {
      dbContext.SaveChanges();
    }
  }
}
