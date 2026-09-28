using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.DeviceProcessor.Repositories
{
    public interface IRepository
    {
        Task UpsertDeviceSummaryAsync(IEnumerable<DeviceSummary> deviceSummaries);
    }
}

namespace uBeac.DeviceProcessor.Repositories.MongoDB
{
    public class Repository : IRepository
    {
        private readonly IMongoDatabase _deviceSummaryDatabase;
        private readonly IMongoCollection<DeviceSummary> DeviceSummaryCollection;

        public Repository(DeviceSummaryDatabase deviceSummaryDatabase)
        {
            _deviceSummaryDatabase = deviceSummaryDatabase.GetMongoDB();
            DeviceSummaryCollection = _deviceSummaryDatabase.GetCollection<DeviceSummary>(typeof(DeviceSummary).Name);
        }
        
        public async Task UpsertDeviceSummaryAsync(IEnumerable<DeviceSummary> deviceSummaries)
        {
            DeviceSummaryCollection.WithWriteConcern(new WriteConcern(1, null, false, true));

            var tasks = new List<Task<UpdateResult>>();

            var update = Builders<DeviceSummary>.Update;

            foreach (var item in deviceSummaries)
            {
                // Update query on device summary
                var filter = Builders<DeviceSummary>.Filter;
                var findDevice = filter.And(filter.Eq(e => e.TeamId, item.TeamId), filter.Eq(e => e.GatewayId, item.GatewayId), filter.Eq(e => e.DeviceId, item.DeviceId));
                var updateSet = update.Set(e => e.LastRequestDate, item.LastRequestDate)
                                      .Set(e => e.FloorId, item.FloorId)
                                      .Inc(e => e.RequestCount, item.RequestCount);

                // Insert query for new device summaries
                var insertCommand = update.SetOnInsert(e => e.FirstRequestDate, item.FirstRequestDate)
                                          .SetOnInsert(e => e.GatewayId, item.GatewayId)
                                          .SetOnInsert(e => e.DeviceId, item.DeviceId)
                                          .SetOnInsert(e => e.Id, item.Id)
                                          .SetOnInsert(e => e.TeamId, item.TeamId)
                                          .SetOnInsert(e => e.DeviceUid, item.DeviceUid);

                // Run all queries
                var updateQueries = update.Combine(new[] { updateSet, insertCommand });
                tasks.Add(DeviceSummaryCollection.UpdateOneAsync(findDevice, updateQueries, new UpdateOptions() { IsUpsert = true }));
            }

            await Task.WhenAll(tasks);
        }

    }
}
