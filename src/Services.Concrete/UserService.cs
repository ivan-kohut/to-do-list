using Entities;
using Repositories;
using System;
using System.Threading.Tasks;

namespace Services
{
  public class UserService : IUserService
  {
    private readonly IUserRepository userRepository;
    private readonly ITransactionManager transactionManager;

    public UserService(IUserRepository userRepository, ITransactionManager transactionManager)
    {
      this.userRepository = userRepository;
      this.transactionManager = transactionManager;
    }

    public async Task SaveAsync(int identityId)
    {
      if (await userRepository.GetUserAsync(identityId) is not null)
      {
        throw new ArgumentException($"User with the identity id already exists: {identityId}", nameof(identityId));
      }

      userRepository.Create(new User { IdentityId = identityId });

      await transactionManager.SaveChangesAsync();
    }
  }
}
