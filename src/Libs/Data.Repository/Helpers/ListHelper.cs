using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Helpers
{
    public static class ListHelper
    {
        public static void Replace<T>(this List<T> source, List<T> target)
        {
            if (source == null || target == null)
                return;
            
            source.Clear();
            source.AddRange(target);
        }

        public static void AllDeleted<T>(this DbContext source, List<T> list)
        {
            list.ForEach(_ => source.Entry(_).State = EntityState.Deleted);
        }

        /// <summary>
        /// Synchronizes a collection by creating, updating, and deleting items
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TDto">DTO type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <param name="originalCollection">Original collection from database</param>
        /// <param name="requestCollection">Request collection with changes</param>
        /// <param name="dbSet">DbSet for adding/removing entities</param>
        /// <param name="getId">Function to get ID from entity or DTO</param>
        /// <param name="createNew">Function to create new entity from DTO</param>
        /// <param name="updateExisting">Action to update existing entity from DTO</param>
        public static async Task SyncCollectionAsync<TEntity, TDto, TKey>(
            ICollection<TEntity> originalCollection,
            IEnumerable<TDto> requestCollection,
            DbSet<TEntity> dbSet,
            Func<object, TKey> getId,
            Func<TDto, TEntity> createNew,
            Action<TDto, TEntity> updateExisting)
            where TEntity : class
            where TKey : IEquatable<TKey>
        {
            if (requestCollection == null)
                return;

            var requestItems = requestCollection.ToList();
            var requestIds = requestItems
                .Select(dto => getId(dto))
                .Where(id => !id.Equals(default(TKey)))
                .ToHashSet();

            // Delete items that are in DB but not in request
            var itemsToDelete = originalCollection
                .Where(entity => !requestIds.Contains(getId(entity)))
                .ToList();

            foreach (var item in itemsToDelete)
            {
                dbSet.Remove(item);
            }

            // Process request items
            foreach (var dto in requestItems)
            {
                var dtoId = getId(dto);

                if (dtoId.Equals(default(TKey)))
                {
                    // Create new item
                    var newEntity = createNew(dto);
                    await dbSet.AddAsync(newEntity);
                }
                else
                {
                    // Update existing item
                    var existingEntity = originalCollection
                        .FirstOrDefault(e => getId(e).Equals(dtoId));

                    if (existingEntity != null)
                    {
                        updateExisting(dto, existingEntity);
                    }
                }
            }
        }
    }
}
