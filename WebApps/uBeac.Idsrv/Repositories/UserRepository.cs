using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using uBeac.Idsrv.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Idsrv.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Guid> InsertAsync(User data, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(User data, CancellationToken cancellationToken = default);
        Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default);
        Task<User> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default);
        Task<IList<User>> GetUsersForClaimAsync(string claimType, string claimValue, CancellationToken cancellationToken = default);
        Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
        Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Idsrv.Repositories.MongoDB
{
    public class UserRepository: IUserRepository
    {
        private readonly IMongoCollection<User> _collection;

        public UserRepository(MainDatabase mainDatabase)
        {
            _collection = mainDatabase.GetMongoDB().GetCollection<User>(typeof(User).Name);
        }

        public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var result = await _collection.FindAsync(filter, cancellationToken: cancellationToken);
            return result.FirstOrDefault();
        }

        public async Task<Guid> InsertAsync(User data, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(data, cancellationToken: cancellationToken);
            return data.Id;
        }

        public async Task<bool> UpdateAsync(User data, CancellationToken cancellationToken = default)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, data.Id);
            var replaceResult = await _collection.ReplaceOneAsync(filter, data, new UpdateOptions { IsUpsert = false });

            return replaceResult.ModifiedCount > 0;

        }

        public async Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            var query = Builders<User>.Filter.Eq(x => x.NormalizedEmail, normalizedEmail);
            var result = await _collection.FindAsync(query, cancellationToken: cancellationToken).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<User> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            var query = Builders<User>.Filter.ElemMatch(x => x.Logins, x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
            var result = await _collection.FindAsync(query, cancellationToken: cancellationToken).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        {
            var query = Builders<User>.Filter.Eq(x => x.NormalizedUserName, normalizedUserName);
            var result = await _collection.FindAsync(query, cancellationToken: cancellationToken).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<IList<User>> GetUsersForClaimAsync(string claimType, string claimValue, CancellationToken cancellationToken = default)
        {
            var query = Builders<User>.Filter.ElemMatch(x => x.Claims, x => x.ClaimValue == claimValue && x.ClaimType == claimType);
            var result = await _collection.FindAsync(query, cancellationToken: cancellationToken).ConfigureAwait(false);

            return result.ToList();
        }

        public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var query = Builders<User>.Filter.ElemMatch(x => x.Roles, x => x == roleName);
            var result = await _collection.FindAsync(query, cancellationToken: cancellationToken).ConfigureAwait(false);

            return (IList<User>)result.ToListAsync();

        }
    }
}
