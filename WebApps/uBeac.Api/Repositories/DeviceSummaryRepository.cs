using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface IDeviceSummaryRepository
    {
        Task<List<DeviceSummary>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<bool> DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<bool> DeleteByGatewayIdAsync(Guid gatewayId, CancellationToken cancellationToken = default);
        Task<bool> DeleteByFloorIdsAsync(IEnumerable<Guid?> floorIds, CancellationToken cancellationToken = default);
        Task<bool> DeleteByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class DeviceSummaryRepository : IDeviceSummaryRepository
    {
        private readonly DeviceSummaryDatabase _deviceSummaryDatabase;
        public DeviceSummaryRepository(DeviceSummaryDatabase deviceSummaryDatabase)
        {
            deviceSummaryDatabase.ThrowIfNull();
            _deviceSummaryDatabase = deviceSummaryDatabase;
        }

        public async Task<bool> DeleteByGatewayIdAsync(Guid gatewayId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<DeviceSummary>.Filter.Eq(x => x.GatewayId, gatewayId);
            var collection = _deviceSummaryDatabase.GetMongoDB().GetCollection<DeviceSummary>(GetCollectionName()).WithWriteConcern(WriteConcern.Unacknowledged);
            await collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);

            // by changing writeConcern to Unacknowledged, we cannot return deleted items
            return true;
        }

        public async Task<bool> DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<DeviceSummary>.Filter.Eq(x => x.TeamId, teamId);            
            var collection = _deviceSummaryDatabase.GetMongoDB().GetCollection<DeviceSummary>(GetCollectionName()).WithWriteConcern(WriteConcern.Unacknowledged);
            await collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);

            // by changing writeConcern to Unacknowledged, we cannot return deleted items
            return true;
        }

        public async Task<List<DeviceSummary>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<DeviceSummary>.Filter.Eq(x => x.TeamId, teamId);
            var collection = _deviceSummaryDatabase.GetMongoDB().GetCollection<DeviceSummary>(GetCollectionName());
            var result = await collection.FindAsync(filter, cancellationToken: cancellationToken);
            return result.ToList();
        }

        public async Task<bool> DeleteByFloorIdsAsync(IEnumerable<Guid?> floorIds, CancellationToken cancellationToken = default)
        {
            var filter = Builders<DeviceSummary>.Filter.In(x => x.FloorId, floorIds);
            var collection = _deviceSummaryDatabase.GetMongoDB().GetCollection<DeviceSummary>(GetCollectionName(), new MongoCollectionSettings { WriteConcern = WriteConcern.Unacknowledged });
            await collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);
            return true;
        }

        public async Task<bool> DeleteByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<DeviceSummary>.Filter.Eq(x => x.DeviceId, deviceId);
            var collection = _deviceSummaryDatabase.GetMongoDB().GetCollection<DeviceSummary>(GetCollectionName(), new MongoCollectionSettings { WriteConcern = WriteConcern.Unacknowledged });
            await collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);

            // by changing writeConcern to Unacknowledged, we cannot return deleted items
            return true;
        }

        private string GetCollectionName()
        {
            return Constant.MONGODB_DEVICE_SUMMARY_COLLECTION_NAME;
        }
       
    }
}
