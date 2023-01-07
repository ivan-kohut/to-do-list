using System;
using System.Threading;
using System.Threading.Tasks;

namespace TodoList.Items.Domain.Shared
{
    public interface IUnitOfWork : IDisposable
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
