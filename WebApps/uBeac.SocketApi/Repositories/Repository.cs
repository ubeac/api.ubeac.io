using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.SocketApi.Repositories
{
    public interface IRepository<TEntity>
    {
        Task<Dictionary<Guid, Guid>> GetAllAsync(CancellationToken cancellationToken = default);
    }
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly IMongoDatabase _dataDatabase;
        private string _typeName = typeof(TEntity).Name;
        private readonly ILogger<Repository<TEntity>> _logger;
        public Repository(MainDatabase dataDatabase, ILogger<Repository<TEntity>> logger)
        {
            _logger = logger;
            _dataDatabase = dataDatabase.GetMongoDB();
        }
        public async Task<Dictionary<Guid, Guid>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<Guid, Guid>();
            try
            {
                var collection = _dataDatabase.GetCollection<TEntity>(_typeName);

                var findOption = new FindOptions<TEntity, CacheObjectModel> { Projection = Builders<TEntity>.Projection.Include(x => x.Id).Include(x => x.TeamId) };
                var list = (await collection.FindAsync(Builders<TEntity>.Filter.Empty, findOption)).ToList();
                result = list.ToDictionary(x => x.Id, x => x.TeamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error in GetAll for {0}", typeof(TEntity).Name));
            }

            return result;
        }

        private class CacheObjectModel
        {
            public Guid Id { get; set; }
            public Guid TeamId { get; set; }
        }
    }        
}
