using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Repositories;
using uBeac.Models;

namespace uBeac.Api.Services
{
    // Services should handle caching 
    public interface IBaseService<TKey, TEntity> : IDisposable where TEntity : IEntity<TKey> where TKey : IEquatable<TKey>
    {
        Task<TKey> AddAsync(TEntity model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(TEntity model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
        //Task<long> DeleteManyAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TEntity> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);
    }

    public class BaseService<TKey, TEntity> : IBaseService<TKey, TEntity> where TEntity : IEntity<TKey> where TKey : IEquatable<TKey>
    {
        protected readonly IGenericRepository<TKey, TEntity> Repository;

        public BaseService(IGenericRepository<TKey, TEntity> repository)
        {
            repository.ThrowIfNull();
            Repository = repository;
        }

        public virtual async Task<TKey> AddAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            return await Repository.InsertAsync(model, cancellationToken);
        }

        public virtual async Task<bool> UpdateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            return await Repository.UpdateAsync(model, cancellationToken);
        }

        public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await Repository.DeleteAsync(id, cancellationToken);
        }

        public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Repository.GetAllAsync(cancellationToken);
        }

        public virtual async Task<TEntity> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await Repository.GetByIdAsync(id, cancellationToken);
        }

        public virtual async Task<List<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
        {
            return await Repository.GetByIdsAsync(ids, cancellationToken);
        }

        //public virtual async Task<long> DeleteManyAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
        //{
        //    return await Repository.DeleteManyAsync(ids, cancellationToken);
        //}

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
