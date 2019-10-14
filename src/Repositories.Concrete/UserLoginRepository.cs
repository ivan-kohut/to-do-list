﻿using Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
  public class UserLoginRepository : IUserLoginRepository
  {
    private readonly AppDbContext dbContext;

    public UserLoginRepository(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public IQueryable<UserLogin> GetAll() => dbContext.UserLogins;

    public async Task<UserLogin?> GetByLoginProviderAndProviderKeyAsync(string loginProvider, string providerKey) =>
      await dbContext
        .UserLogins
        .SingleOrDefaultAsync(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);

    public async Task CreateAsync(UserLogin userLogin) => await dbContext.AddAsync(userLogin);
  }
}
