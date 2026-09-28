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
    public interface IAccessRepository
    {
        Task<List<Access>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<List<Access>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<long> DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<Guid> InsertAsync(Access data, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Access data, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Access> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistAsync(Guid userId, Guid teamId, CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class AccessRepository : IAccessRepository
    {
        private readonly MainDatabase _database;
        private readonly IMongoCollection<Access> _collection;

        public AccessRepository(MainDatabase database)
        {
            database.ThrowIfNull();
            _database = database;
            _collection = _database.GetMongoDB().GetCollection<Access>(typeof(Access).Name);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            id.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<Access>.Filter.Eq(x => x.Id, id);
            var result = await _collection.DeleteOneAsync(filter, cancellationToken: cancellationToken);
            return result is null ? false : true;
        }

        public async Task<long> DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            teamId.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<Access>.Filter.Eq(x => x.TeamId, teamId);
            var result = (await _collection.DeleteManyAsync(filter, cancellationToken: cancellationToken)).DeletedCount;
            return result;
        }

        public async Task<Access> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            id.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<Access>.Filter.Eq(x => x.Id, id);
            var result = await _collection.FindAsync(filter, cancellationToken: cancellationToken);
            return result.SingleOrDefault();
        }

        public async Task<List<Access>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            teamId.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<Access>.Filter.Eq(x => x.TeamId, teamId);
            var result = await _collection.FindAsync(filter, cancellationToken: cancellationToken);
            return result.ToList();
        }

        public async Task<List<Access>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            userId.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<Access>.Filter.Eq(x => x.UserId, userId);
            var result = await _collection.FindAsync(filter, cancellationToken: cancellationToken);
            return result.ToList();
        }

        public async Task<Guid> InsertAsync(Access data, CancellationToken cancellationToken = default)
        {
            data.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();

            await _collection.InsertOneAsync(data, cancellationToken: cancellationToken);
            return data.Id;
        }

        public async Task<bool> UpdateAsync(Access data, CancellationToken cancellationToken = default)
        {
            data.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();
            
            var filter = Builders<Access>.Filter.Eq(x => x.Id, data.Id);
            var replaceResult = await _collection.ReplaceOneAsync(filter, data, new UpdateOptions { IsUpsert = false });

            return replaceResult.ModifiedCount > 0;
        }

        public async Task<bool> ExistAsync(Guid userId, Guid teamId, CancellationToken cancellationToken = default)
        {
            userId.ThrowIfNull();
            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<Access>.Filter;
            var query = filter.And(filter.Eq(x => x.UserId, userId), filter.Eq(x => x.TeamId, teamId));
            var result = await _collection.FindAsync(query, cancellationToken: cancellationToken);

            return result.ToList().Count > 0;
        }
    }
}

