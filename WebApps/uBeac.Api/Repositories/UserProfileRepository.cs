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
    public interface IUserProfileRepository
    {
        Task<UserProfile> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<List<UserProfile>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
        Task<UserProfile> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly MainDatabase _database;
        private readonly IMongoCollection<UserProfile> _collection;

        public UserProfileRepository(MainDatabase database)
        {
            _database = database;
            _collection = _database.GetMongoDB().GetCollection<UserProfile>(typeof(UserProfile).Name);
        }

        public async Task<UserProfile> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var filter = Builders<UserProfile>.Filter.Eq(x => x.Username, email);
            var result = await _collection.FindAsync(filter, cancellationToken: cancellationToken);
            return result.FirstOrDefault();
        }

        public async Task<UserProfile> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<UserProfile>.Filter.Eq(x => x.Id, userId);
            var result = await _collection.FindAsync(filter, cancellationToken: cancellationToken);
            return result.FirstOrDefault();
        }

        public async Task<List<UserProfile>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            var filter = Builders<UserProfile>.Filter.In(x => x.Id, userIds);
            var result = await _collection.FindAsync(filter, cancellationToken: cancellationToken);
            return result.ToList();
        }
    }
}
