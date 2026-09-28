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
    public interface IGatewayRepository : IBaseEntityRepository<Gateway>
    {
        Task<Gateway> GetByUrlAsync(Guid teamId, string url, CancellationToken cancellationToken = default);
        Task<bool> ResetFloorDataAsync(Guid floorId, CancellationToken cancellationToken = default);
        Task<bool> ResetFloorDataAsync(IEnumerable<Guid?> floorIds, CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class GatewayRepository : BaseEntityRepository<Gateway>, IGatewayRepository
    {
        public GatewayRepository(MainDatabase database) : base(database)
        {
        }
        
        public async Task<bool> ResetFloorDataAsync(Guid floorId, CancellationToken cancellationToken = default)
        {
            var query = Builders<Gateway>.Filter.Eq(x => x.FloorId, floorId);
            var updateQuery = Builders<Gateway>.Update.Set(x => x.FloorId, null).Set(x => x.X, 50).Set(x => x.Y, 50);
            var result = await Collection.UpdateManyAsync(query, updateQuery, cancellationToken: cancellationToken);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ResetFloorDataAsync(IEnumerable<Guid?> floorIds, CancellationToken cancellationToken = default)
        {
            var query = Builders<Gateway>.Filter.In(x => x.FloorId, floorIds);
            var updateQuery = Builders<Gateway>.Update.Set(x => x.FloorId, null).Set(x => x.X, 50).Set(x => x.Y, 50);
            var result = await Collection.UpdateManyAsync(query, updateQuery, cancellationToken: cancellationToken);
            return result.ModifiedCount > 0;
        }

        public async Task<Gateway> GetByUrlAsync(Guid teamId, string url, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Gateway>.Filter;
            var query = filter.And(filter.Eq(x => x.TeamId, teamId), filter.Eq(x => x.Url, url));
            var result = await Collection.FindAsync(query, cancellationToken: cancellationToken);
            return result.FirstOrDefault();
        }
        
    }
}
