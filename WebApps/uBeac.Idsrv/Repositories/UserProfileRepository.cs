using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Idsrv.Repositories
{
    public interface IUserProfileRepository
    {
        Task<bool> UpdateAsync(UserProfile data, CancellationToken cancellationToken = default);
        Task<UserProfile> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UserProfile> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task UpdateLastActivityDate(Guid userId, CancellationToken cancellationToken = default);
    }
}
namespace uBeac.Idsrv.Repositories.MongoDB
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly MainDatabase _mainDatabase;
        public readonly IMongoCollection<UserProfile> _collection;

        public UserProfileRepository(MainDatabase mainDatabase)
        {
            _mainDatabase = mainDatabase;
            _collection = _mainDatabase.GetMongoDB().GetCollection<UserProfile>(typeof(UserProfile).Name);
        }

        public async Task<UserProfile> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            // return empty if namespace is empty without query
            if (string.IsNullOrEmpty(email))
                return null;

            var query = Builders<UserProfile>.Filter.Eq(x => x.Email, email);

            var result = await _collection.FindAsync(query, cancellationToken: cancellationToken);

            return result.FirstOrDefault();

        }

        public async Task<UserProfile> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // return empty if namespace is empty without query
            if (id == Guid.Empty)
                return null;

            var query = Builders<UserProfile>.Filter.Eq(x => x.Id, id);

            var result = await _collection.FindAsync(query, cancellationToken: cancellationToken);

            return result.FirstOrDefault();
        }

        public async Task<bool> UpdateAsync(UserProfile data, CancellationToken cancellationToken = default)
        {
            var filter = Builders<UserProfile>.Filter.Eq(x => x.Id, data.Id);
            var replaceResult = await _collection.ReplaceOneAsync(filter, data, new UpdateOptions { IsUpsert = true });

            return replaceResult.ModifiedCount > 0;
        }

        public async Task UpdateLastActivityDate(Guid userId, CancellationToken cancellationToken = default)
        {
            var updateQuery = Builders<UserProfile>.Update.Set(x => x.LastActivityDate, DateTime.UtcNow);
            var findQuery = Builders<UserProfile>.Filter.Eq(x => x.Id, userId);
            await _collection.UpdateOneAsync(findQuery, updateQuery);
        }
    }
}
