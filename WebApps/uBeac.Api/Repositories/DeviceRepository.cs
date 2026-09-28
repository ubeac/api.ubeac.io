using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface IDeviceRepository : IBaseEntityRepository<Device>
    {
        Task<Device> GetByUidIdAsync(Guid teamId, string deviceUid, CancellationToken cancellationToken = default);
        Task<bool> ResetFloorDataAsync(Guid floorId, CancellationToken cancellationToken = default);
        Task<bool> ResetFloorDataAsync(IEnumerable<Guid?> floorIds, CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class DeviceRepository : BaseEntityRepository<Device>, IDeviceRepository
    {
        public DeviceRepository(MainDatabase database) : base(database)
        {
        }

        public async Task<Device> GetByUidIdAsync(Guid teamId, string deviceUid, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Device>.Filter;
            var query = filter.And(filter.Eq(x => x.TeamId, teamId), filter.Eq(x => x.Uid, deviceUid));
            var result = await Collection.FindAsync(query, cancellationToken: cancellationToken);
            return result.SingleOrDefault();
        }

        public async Task<bool> ResetFloorDataAsync(Guid floorId, CancellationToken cancellationToken = default)
        {
            var query = Builders<Device>.Filter.Eq(x => x.FloorId, floorId);
            var updateQuery = Builders<Device>.Update.Set(x => x.FloorId, null).Set(x => x.X, 50).Set(x => x.Y, 50);
            var result = await Collection.UpdateManyAsync(query, updateQuery, cancellationToken: cancellationToken);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ResetFloorDataAsync(IEnumerable<Guid?> floorIds, CancellationToken cancellationToken = default)
        {
            var query = Builders<Device>.Filter.In(x => x.FloorId, floorIds);
            var updateQuery = Builders<Device>.Update.Set(x => x.FloorId, null).Set(x => x.X, 50).Set(x => x.Y, 50);
            var result = await Collection.UpdateManyAsync(query, updateQuery, cancellationToken: cancellationToken);
            return result.ModifiedCount > 0;
        }
    }
}
