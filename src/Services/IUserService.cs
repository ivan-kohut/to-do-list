using System.Threading.Tasks;

namespace Services
{
  public interface IUserService
  {
    Task SaveAsync(int identityId);
  }
}
