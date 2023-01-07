using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TodoList.Items.Domain.Aggregates.UserAggregate;

namespace TodoList.Items.Infrastructure.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(ItemsDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<User?> GetUserAsync(int identityId) =>
            await dbContext
                .Users
                .SingleOrDefaultAsync(u => u.IdentityId == identityId);
    }
}
