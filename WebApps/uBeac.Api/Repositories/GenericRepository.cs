using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface IGenericRepository<TKey, TEntity> : IDisposable
         where TEntity : IEntity<TKey>
         where TKey : IEquatable<TKey>
    {
        Task<TKey> InsertAsync(TEntity data, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(TEntity data, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
        Task<bool> DeleteManyAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TEntity> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public abstract class GenericRepository<TKey, TEntity> : IGenericRepository<TKey, TEntity> 
            where TEntity : IEntity<TKey> 
            where TKey : IEquatable<TKey>
    {

        public readonly MainDatabase Database;
        public readonly IMongoCollection<TEntity> Collection;
        public readonly IMongoCollection<BsonDocument> BsonCollection;

        public GenericRepository(MainDatabase database)
        {
            database.ThrowIfNull();
            CollectionName.ThrowIfNull();

            Database = database;
            Collection = Database.GetMongoDB().GetCollection<TEntity>(CollectionName);
            BsonCollection = Database.GetMongoDB().GetCollection<BsonDocument>(CollectionName);

            EnsureIndicesCreatedAsync().GetAwaiter().GetResult();
        }

        public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            id.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
            var result = await Collection.DeleteOneAsync(filter, cancellationToken: cancellationToken);
            return result.DeletedCount > 0;
        }

        public virtual async Task<TEntity> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            id.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
            var result = await Collection.FindAsync(filter, cancellationToken: cancellationToken);
            return result.FirstOrDefault();
        }

        public virtual async Task<List<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ids.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<TEntity>.Filter.In(x => x.Id, ids);
            var result = await Collection.FindAsync(filter, cancellationToken: cancellationToken);
            return result.ToList();
        }

        public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            return await Collection.AsQueryable().ToListAsync();
        }

        public virtual async Task<TKey> InsertAsync(TEntity data, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            data.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();

            await Collection.InsertOneAsync(data, cancellationToken: cancellationToken);
            return data.Id;
        }

        public virtual async Task<bool> UpdateAsync(TEntity data, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            data.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<TEntity>.Filter.Eq(x => x.Id, data.Id);
            var replaceResult = await Collection.ReplaceOneAsync(filter, data, new UpdateOptions { IsUpsert = false });

            return replaceResult.ModifiedCount > 0;
        }

        public virtual async Task<bool> DeleteManyAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<TEntity>.Filter.In(x => x.Id, ids);
            var result = await Collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);
            return result.IsAcknowledged;
        }

        protected abstract string CollectionName { get; }

        #region Initialize DB

        private static bool _initialized = false;
        private static object _initializationLock = new object();
        private static object _initializationTarget;

        protected virtual async Task EnsureIndicesCreatedAsync()
        {
            var obj = LazyInitializer.EnsureInitialized(ref _initializationTarget, ref _initialized, ref _initializationLock, () =>
            {
                return EnsureIndicesCreatedDefinitionsAsync();
            });

            if (obj != null)
            {
                var taskToAwait = (Task)obj;
                await taskToAwait.ConfigureAwait(false);
            }
        }

        protected virtual async Task EnsureIndicesCreatedDefinitionsAsync()
        {
            await Task.FromResult(0);
        }

        #endregion Initialize DB 

        #region Disposable

        private bool _disposed = false;
        public void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }

        #endregion

    }
}