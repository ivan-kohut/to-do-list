using Entities;
using Repositories;
using Services.Exceptions;
using System;
using System.Threading.Tasks;

namespace Services
{
  public class UserRoleService : IUserRoleService
  {
    private readonly IUserService userService;
    private readonly IRoleRepository roleRepository;
    private readonly IUserRoleRepository userRoleRepository;
    private readonly ITransactionManager transactionManager;

    public UserRoleService(IUserService userService,
                           IRoleRepository roleRepository,
                           IUserRoleRepository userRoleRepository,
                           ITransactionManager transactionManager)
    {
      this.userService = userService;
      this.roleRepository = roleRepository;
      this.userRoleRepository = userRoleRepository;
      this.transactionManager = transactionManager;
    }

    public async Task CreateAsync(int userId, string roleName)
    {
      _ = await userService.GetByIdAsync(userId);

      Role? role = await roleRepository.GetByNameAsync(roleName);

      if (role == null)
      {
        throw new EntityNotFoundException($"Role with name {roleName} is not found");
      }

      UserRole? userRole = await userRoleRepository.GetByUserIdAndRoleIdAsync(userId, role.Id);

      if (userRole != null)
      {
        throw new ArgumentException($"User has role with name {roleName} already");
      }

      await userRoleRepository.CreateAsync(new UserRole { UserId = userId, RoleId = role.Id });

      await transactionManager.SaveChangesAsync();
    }
  }
}
