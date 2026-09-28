using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface IBaseEntityRepository<TEntity> : IGenericRepository<Guid, TEntity> where TEntity : IBaseEntity
    {
        Task<bool> DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class BaseEntityRepository<TEntity> : GenericRepository<Guid, TEntity>, IBaseEntityRepository<TEntity> where TEntity : IBaseEntity
    {
        public BaseEntityRepository(MainDatabase database) : base(database)
        {
        }

        protected override string CollectionName => typeof(TEntity).Name;

        public override async Task<Guid> InsertAsync(TEntity data, CancellationToken cancellationToken = default)
        {
            return await base.InsertAsync(data, cancellationToken);
        }
        
        public virtual async Task<bool> DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.TeamId, teamId);            
            var result = await Collection.WithWriteConcern(WriteConcern.Unacknowledged).DeleteManyAsync(filter, cancellationToken: cancellationToken);
            return result.IsAcknowledged;
        }

        public virtual async Task<List<TEntity>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.TeamId, teamId);
            var result = await Collection.FindAsync(filter, cancellationToken: cancellationToken);
            return result.ToList();
        }

    }
}