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
    public interface IGatewayDataRepository
    {
        Task CreateCollectionByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<bool> DeleteByTeamIdAsync(Guid teamId, IEnumerable<Guid> gatewayIds, CancellationToken cancellationToken = default);
        Task<bool> DeleteByTeamIdAsync(Guid teamId, Guid gatewayId, CancellationToken cancellationToken = default);        
        Task<List<GatewayData>> GetDataAsync(Guid teamId, Guid gatewayId, DateTime fromDate, DateTime toDate, int pageSize, int pageCount, CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class GatewayDataRepository : IGatewayDataRepository
    {
        private readonly GatewayDataDatabase _gatewayDataDatabase;

        public GatewayDataRepository(GatewayDataDatabase gatewayDataDatabase)
        {
            gatewayDataDatabase.ThrowIfNull();
            _gatewayDataDatabase = gatewayDataDatabase;
        }

        public async Task CreateCollectionByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            var collectionName = GetCollectionName(teamId);
            // Create collection
            await _gatewayDataDatabase.GetMongoDB().CreateCollectionAsync(collectionName, null, cancellationToken);

            // Create Indices
            var listIndex = new List<CreateIndexModel<GatewayData>>();
            var newGatewayDataCollection = _gatewayDataDatabase.GetMongoDB().GetCollection<GatewayData>(collectionName);

            var gatewayDataindexBuilder = Builders<GatewayData>.IndexKeys;
            listIndex.Add(new CreateIndexModel<GatewayData>(gatewayDataindexBuilder.Descending(x => x.DateTime),
                new CreateIndexOptions { Background = true, Name = "DateTime_Index", ExpireAfter = new TimeSpan(90,0,0,0) }));                       

            //listIndex.Add(new CreateIndexModel<GatewayData>(gatewayDataindexBuilder.Ascending(x => x.GatewayId),
            //    new CreateIndexOptions { Background = true, Name = "GatewayId_Index" }));

            listIndex.Add(new CreateIndexModel<GatewayData>(gatewayDataindexBuilder.Descending(x => x.DateTime).Ascending(x => x.GatewayId),
                new CreateIndexOptions { Background = true, Name = "DateTime_GatewayId_Index" }));

            await newGatewayDataCollection.Indexes.CreateManyAsync(listIndex, cancellationToken);
        }

        public async Task DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            await _gatewayDataDatabase.GetMongoDB().DropCollectionAsync(GetCollectionName(teamId), cancellationToken);
        }

        public async Task<bool> DeleteByTeamIdAsync(Guid teamId, Guid gatewayId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<GatewayData>.Filter.Eq(x => x.GatewayId, gatewayId);
            var collection = _gatewayDataDatabase.GetMongoDB().GetCollection<GatewayData>(GetCollectionName(teamId)).WithWriteConcern(WriteConcern.Unacknowledged);
            await collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);

            return true;
        }

        public async Task<bool> DeleteByTeamIdAsync(Guid teamId, IEnumerable<Guid> gatewayIds, CancellationToken cancellationToken = default)
        {
            if (gatewayIds.Count() == 0)
                return true;

            var filter = Builders<GatewayData>.Filter.In(x => x.GatewayId, gatewayIds);
            var collection = _gatewayDataDatabase.GetMongoDB().GetCollection<GatewayData>(GetCollectionName(teamId)).WithWriteConcern(WriteConcern.Unacknowledged);
            await collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);

            return true;
        }

        public async Task<List<GatewayData>> GetDataAsync(Guid teamId, Guid gatewayId, DateTime fromDate, DateTime toDate, int pageSize, int pageCount, CancellationToken cancellationToken = default)
        {
            var filter = Builders<GatewayData>.Filter;
            
            // SensorId query
            var queryGatewayId = filter.Eq(t => t.GatewayId, gatewayId);

            // Date query
            var queryDate = filter.And(filter.Gte(t => t.DateTime, fromDate), filter.Lte(t => t.DateTime, toDate));            

            var query = filter.And(queryDate, queryGatewayId);

            var options = new FindOptions<GatewayData>()
            {
                Sort = Builders<GatewayData>.Sort.Descending(t => t.DateTime),
                Skip = (pageCount - 1) * pageSize,
                Limit = pageSize,
                Projection = Builders<GatewayData>.Projection.Exclude("_id")
            };

            var collection = _gatewayDataDatabase.GetMongoDB().GetCollection<GatewayData>(GetCollectionName(teamId));
            var result = await collection.FindAsync(query, options);
            return result.ToList();
        }

        private string GetCollectionName(Guid teamId)
        {
            return Constant.MONGODB_GATEWAY_DATA_COLLECTION_NAME + "_" + teamId.ToString().Replace('-', '_');
        }

    }
}
