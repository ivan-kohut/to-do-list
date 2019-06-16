using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
  public class UserService : IUserService
  {
    private const int adminId = 1;

    private readonly IUserRepository userRepository;
    private readonly IUserLoginRepository userLoginRepository;
    private readonly ITransactionManager transactionManager;

    public UserService(IUserRepository userRepository, 
                       IUserLoginRepository userLoginRepository,
                       ITransactionManager transactionManager)
    {
      this.userRepository = userRepository;
      this.userLoginRepository = userLoginRepository;
      this.transactionManager = transactionManager;
    }

    public async Task<UserDTO> GetByIdAsync(int id)
    {
      User user = await userRepository.GetByIdAsync(id);

      if (user == null)
      {
        throw new EntityNotFoundException($"User with id {id} is not found");
      }

      return new UserDTO
      {
        Id = user.Id,
        Name = user.UserName,
        Email = user.Email,
        IsRegisteredInSystem = user.PasswordHash != null,
        IsEmailConfirmed = user.EmailConfirmed,
        LoginProviders = await userLoginRepository
          .GetAll()
          .Where(l => l.UserId == user.Id)
          .Select(l => l.LoginProvider)
          .ToListAsync()
      };
    }

    public async Task<IEnumerable<UserDTO>> GetAllAsync()
    {
      return await userRepository
        .GetAll()
        .Where(u => u.Id != adminId)
        .Select(u => new UserDTO
        {
          Id = u.Id,
          Name = u.UserName,
          Email = u.Email,
          IsRegisteredInSystem = u.PasswordHash != null,
          IsEmailConfirmed = u.EmailConfirmed,
          LoginProviders = u.UserLogins
            .Select(l => l.LoginProvider)
            .ToList()
        })
        .ToListAsync();
    }

    public async Task DeleteAsync(int id)
    {
      if (id == adminId)
      {
        throw new ArgumentException($"You can not delete user with id {id}");
      }

      User user = await userRepository.GetByIdAsync(id);

      if (user == null)
      {
        throw new EntityNotFoundException($"User with id {id} is not found");
      }

      userRepository.Delete(user);

      await transactionManager.SaveChangesAsync();
    }
  }
}
