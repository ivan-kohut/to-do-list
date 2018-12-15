using System.Threading.Tasks;

namespace Repositories
{
  public abstract class BaseRepository
  {
    private readonly AppDbContext dbContext;

    protected BaseRepository(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public async Task SaveChangesAsync()
    {
      await dbContext.SaveChangesAsync();
    }
  }
}
