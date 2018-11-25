using System.Threading.Tasks;

namespace Repositories
{
  public interface IDbTransactionManager
  {
    Task SaveChangesAsync();
  }
}
