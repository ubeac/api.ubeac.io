using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface ISensorRepository : IBaseEntityRepository<Sensor>
    {
        Task<List<Sensor>> GetByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken);
        Task<List<Sensor>> GetByDeviceIdsAsync(IEnumerable<Guid> deviceIds, CancellationToken cancellationToken);
        Task<Sensor> GetByUidIdAsync(Guid deviceId, string sensorUid, CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class SensorRepository : BaseEntityRepository<Sensor>, ISensorRepository
    {
        public SensorRepository(MainDatabase database) : base(database)
        {
        }

        public async Task<List<Sensor>> GetByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken)
        {
            var query = Builders<Sensor>.Filter.Eq(x => x.DeviceId, deviceId);
            return (await Collection.FindAsync(query, cancellationToken: cancellationToken)).ToList();
        }

        public async Task<List<Sensor>> GetByDeviceIdsAsync(IEnumerable<Guid> deviceIds, CancellationToken cancellationToken)
        {
            var query = Builders<Sensor>.Filter.In(x => x.DeviceId, deviceIds);
            return (await Collection.FindAsync(query, cancellationToken: cancellationToken)).ToList();
        }

        public async Task<Sensor> GetByUidIdAsync(Guid deviceId, string sensorUid, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Sensor>.Filter;
            var query = filter.And(filter.Eq(x => x.DeviceId, deviceId), filter.Eq(x => x.Uid, sensorUid));
            var result = await Collection.FindAsync(query, cancellationToken: cancellationToken);
            return result.SingleOrDefault();
        }
    }
}
