using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TodoList.Items.Domain.Aggregates.UserAggregate;

namespace TodoList.Items.Infrastructure.Repositories
{
    public class UserRepository(ItemsDbContext dbContext) : RepositoryBase<User>(dbContext), IUserRepository
    {
        public async Task<User?> GetUserAsync(int identityId) =>
            await dbContext.Users
                .SingleOrDefaultAsync(u => u.IdentityId == identityId);
    }
}
