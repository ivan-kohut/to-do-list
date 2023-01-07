using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Infrastructure
{
    public class ItemsDbContext : DbContext, IUnitOfWork
    {
        public DbSet<Item> Items { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<ItemStatus> ItemStatuses { get; set; } = null!;

        public ItemsDbContext(DbContextOptions<ItemsDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) => await SaveChangesAsync(cancellationToken);
    }
}
