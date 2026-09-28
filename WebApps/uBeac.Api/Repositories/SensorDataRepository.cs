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
    public interface ISensorDataRepository
    {
        Task CreateCollectionByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<bool> DeleteByTeamIdAsync(Guid teamId, IEnumerable<Guid> sensorIds, CancellationToken cancellationToken = default);        
        Task<bool> DeleteByTeamIdAsync(Guid teamId, Guid gatewayId, CancellationToken cancellationToken = default);
        Task<List<SensorData>> GetDataAsync(Guid teamId, IEnumerable<Guid> sensorIds, DateTime fromDate, DateTime toDate, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class SensorDataRepository : ISensorDataRepository
    {
        private readonly SensorDataDatabase _sensorDataDatabase;
        public SensorDataRepository(SensorDataDatabase sensorDataDatabase)
        {
            sensorDataDatabase.ThrowIfNull();
            _sensorDataDatabase = sensorDataDatabase;
        }

        public async Task CreateCollectionByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            var collectionName = GetCollectionName(teamId);
            // Create collection
            await _sensorDataDatabase.GetMongoDB().CreateCollectionAsync(collectionName, null, cancellationToken);

            // Create Indices
            var newSensorDataCollection = _sensorDataDatabase.GetMongoDB().GetCollection<SensorData>(collectionName);

            var sensorDataIndexBuilder = Builders<SensorData>.IndexKeys;
            var listIndex = new List<CreateIndexModel<SensorData>>();

            listIndex.Add(new CreateIndexModel<SensorData>(sensorDataIndexBuilder.Descending("DateTime"),
                new CreateIndexOptions { Name = "DateTime_Index", Background = true, ExpireAfter = new TimeSpan(90, 0, 0, 0) }));

            //listIndex.Add(new CreateIndexModel<SensorData>(sensorDataIndexBuilder.Ascending("SensorId"),
            //    new CreateIndexOptions { Name = "SensorId_Index", Background = true }));

            listIndex.Add(new CreateIndexModel<SensorData>(sensorDataIndexBuilder.Ascending("SensorId").Descending("DateTime"),
                new CreateIndexOptions { Name = "SensorId_DateTime_Index", Background = true }));

            listIndex.Add(new CreateIndexModel<SensorData>(sensorDataIndexBuilder.Ascending("GatewayId"),
                new CreateIndexOptions { Name = "GatewayId_Index", Background = true }));

            await newSensorDataCollection.Indexes.CreateManyAsync(listIndex, cancellationToken);
        }

        public async Task DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            // todo: should we get the count of sensor data prior to delete and return that?
            await _sensorDataDatabase.GetMongoDB().DropCollectionAsync(GetCollectionName(teamId), cancellationToken);
        }

        public async Task<bool> DeleteByTeamIdAsync(Guid teamId, IEnumerable<Guid> sensorIds, CancellationToken cancellationToken = default)
        {
            if (sensorIds.Count() == 0)
                return true;

            var filter = Builders<SensorData>.Filter.In(x => x.SensorId, sensorIds);
            var collection = _sensorDataDatabase.GetMongoDB().GetCollection<SensorData>(GetCollectionName(teamId), new MongoCollectionSettings { WriteConcern = WriteConcern.Unacknowledged });
            await collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);

            return true;
        }

        public async Task<bool> DeleteByTeamIdAsync(Guid teamId, Guid gatewayId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<SensorData>.Filter.Eq(x => x.GatewayId, gatewayId);
            var collection = _sensorDataDatabase.GetMongoDB().GetCollection<SensorData>(GetCollectionName(teamId), new MongoCollectionSettings { WriteConcern = WriteConcern.Unacknowledged });
            await collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);

            return true;
        }

        public async Task<List<SensorData>> GetDataAsync(Guid teamId, IEnumerable<Guid> sensorIds, DateTime fromDate, DateTime toDate, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
        {
            var filter = Builders<SensorData>.Filter;
            var querySensorId = filter.Empty;

            // Date query
            var queryDate = filter.And(filter.Gte(t => t.DateTime, fromDate), filter.Lte(t => t.DateTime, toDate));

            // SensorId query
            querySensorId = filter.In(t => t.SensorId, sensorIds);

            var query = filter.And(queryDate, querySensorId);

            var options = new FindOptions<SensorData>()
            {
                Sort = Builders<SensorData>.Sort.Descending(t => t.DateTime),
                Skip = (pageNumber - 1) * pageSize,
                Limit = pageSize,
                Projection = Builders<SensorData>.Projection.Exclude("_id")
            };

            var collection = _sensorDataDatabase.GetMongoDB().GetCollection<SensorData>(GetCollectionName(teamId));
            var result = await collection.FindAsync(query, options);
            return result.ToList();
        }

        private string GetCollectionName(Guid teamId)
        {
            return Constant.MONGODB_SENSOR_DATA_COLLECTION_NAME + "_" + teamId.ToString().Replace('-', '_');
        }

    }
}
