﻿using Microsoft.EntityFrameworkCore;

namespace TodoList.Items.IntegrationTests.Extensions
{
    public static class DbContextExtensions
    {
        public static void Rollback<T>(this DbContext dbContext) where T : class
        {
            DbSet<T> entities = dbContext.Set<T>();
            entities.RemoveRange(entities);
        }
    }
}
