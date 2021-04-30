using System.Threading.Tasks;

namespace TodoList.Items.Domain.Aggregates.UserAggregate
{
  public interface IUserRepository
  {
    Task<User?> GetUserAsync(int identityId);
    void Create(User user);
  }
}
