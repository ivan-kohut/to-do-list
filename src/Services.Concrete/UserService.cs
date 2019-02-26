using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Exceptions;
using System.Collections.Generic;
using System.Linq;
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

    public async Task<IEnumerable<UserDTO>> GetAllAsync()
    {
      return await userRepository
        .GetAll()
        .Select(u => new UserDTO
        {
          Id = u.Id,
          Name = u.UserName,
          Email = u.Email,
          IsEmailConfirmed = u.EmailConfirmed
        })
        .ToListAsync();
    }

    public async Task DeleteAsync(int id)
    {
      User user = await userRepository.GetByIdAsync(id);

      if (user == null)
        throw new EntityNotFoundException($"User with id {id} is not found");

      userRepository.Delete(user);

      await transactionManager.SaveChangesAsync();
    }
  }
}
