using System.Threading.Tasks;
using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Domain.Aggregates.UserAggregate
{
  public interface IUserRepository : IRepository<User>
  {
    Task<User?> GetUserAsync(int identityId);
    void Create(User user);
  }
}
