using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Security
{
    public interface IAccessRepository
    {
        Task<List<Access>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Security
{
    public class AccessRepository : IAccessRepository
    {
        private readonly MainDatabase _database;
        private readonly IMongoCollection<Access> _collection;

        public AccessRepository(MainDatabase database)
        {
            _database = database;
            _collection = _database.GetMongoDB().GetCollection<Access>(typeof(Access).Name);
        }
        
        public async Task<List<Access>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<Access>.Filter.Eq(x => x.UserId, userId);
            var result = await _collection.FindAsync(filter, cancellationToken: cancellationToken);
            return result.ToList();
        }
        
    }
}

