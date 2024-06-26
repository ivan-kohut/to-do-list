﻿using TodoList.Items.Domain.Shared;

namespace TodoList.Items.Infrastructure.Repositories
{
    public abstract class RepositoryBase<T>(ItemsDbContext dbContext) : IRepository<T> where T : class, IAggregateRoot
    {
        protected readonly ItemsDbContext dbContext = dbContext;

        public void Create(T aggregateRoot) => dbContext.Add(aggregateRoot);
    }
}
