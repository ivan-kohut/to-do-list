using System.Threading.Tasks;

namespace Repositories
{
  public interface IRepository
  {
    Task SaveChangesAsync();
  }
}
