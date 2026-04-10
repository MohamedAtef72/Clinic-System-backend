using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clinic_System.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Extensions
{
    /// <summary>
    /// Extensions for handling soft delete operations.
    /// Provides helper methods for soft deleting entities safely.
    /// </summary>
    public static class SoftDeleteExtensions
    {
        /// <summary>
        /// Soft delete a single entity and save changes.
        /// </summary>
        public static async Task SoftDeleteAsync<T>(this DbSet<T> dbSet, DbContext context, T entity) where T : BaseEntity
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            dbSet.Update(entity);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Soft delete multiple entities.
        /// </summary>
        public static async Task SoftDeleteRangeAsync<T>(this DbSet<T> dbSet, DbContext context, IEnumerable<T> entities) where T : BaseEntity
        {
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
            }
            dbSet.UpdateRange(entities);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Permanently delete a soft-deleted entity (hard delete).
        /// Use sparingly and only for audit cleanup.
        /// </summary>
        public static async Task HardDeleteAsync<T>(this DbSet<T> dbSet, DbContext context, T entity) where T : BaseEntity
        {
            dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Restore a soft-deleted entity.
        /// </summary>
        public static async Task RestoreAsync<T>(this DbSet<T> dbSet, DbContext context, T entity) where T : BaseEntity
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            dbSet.Update(entity);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Query including soft-deleted records (ignores global filter).
        /// </summary>
        public static IQueryable<T> IncludingSoftDeleted<T>(this DbSet<T> dbSet) where T : BaseEntity
        {
            return dbSet.IgnoreQueryFilters();
        }

        /// <summary>
        /// Query only soft-deleted records.
        /// </summary>
        public static IQueryable<T> OnlySoftDeleted<T>(this DbSet<T> dbSet) where T : BaseEntity
        {
            return dbSet.IgnoreQueryFilters().Where(e => e.IsDeleted);
        }
    }
}