using System.Threading.Tasks;

namespace Repositories
{
  public interface ITransactionManager
  {
    Task SaveChangesAsync();
  }
}
