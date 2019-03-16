using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Exceptions;
using System.Threading.Tasks;

namespace Services
{
  public class UserLoginService : IUserLoginService
  {
    private readonly IUserService userService;
    private readonly IUserLoginRepository userLoginRepository;
    private readonly ITransactionManager transactionManager;

    public UserLoginService(IUserService userService,
                            IUserLoginRepository userLoginRepository,
                            ITransactionManager transactionManager)
    {
      this.userService = userService;
      this.userLoginRepository = userLoginRepository;
      this.transactionManager = transactionManager;
    }

    public async Task CreateAsync(int userId, string loginProvider, string providerKey)
    {
      UserDTO user = await userService.GetByIdAsync(userId);

      UserLogin userLogin = await userLoginRepository.GetByLoginProviderAndProviderKeyAsync(loginProvider, providerKey);

      if (userLogin != null)
      {
        throw new EntityNotFoundException($"User login exists with provider {loginProvider} and provider key {providerKey} already");
      }

      await userLoginRepository.CreateAsync(new UserLogin
      {
        UserId = user.Id,
        LoginProvider = loginProvider,
        ProviderDisplayName = loginProvider,
        ProviderKey = providerKey
      });

      await transactionManager.SaveChangesAsync();
    }

    public async Task<bool> ExistsWithUserIdAndLoginProviderAsync(int userId, string loginProvider)
    {
      return await userLoginRepository
        .GetAll()
        .AnyAsync(p => p.UserId == userId && p.LoginProvider == loginProvider);
    }
  }
}
