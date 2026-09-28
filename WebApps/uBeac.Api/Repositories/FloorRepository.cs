using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface IFloorRepository : IBaseEntityRepository<Floor>
    {
        Task<List<Floor>> GetByBuildingIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class FloorReposiroty : BaseEntityRepository<Floor>, IFloorRepository
    {
        public FloorReposiroty(MainDatabase database) : base(database)
        {
        }

        public async Task<List<Floor>> GetByBuildingIdsAsync(IEnumerable<Guid> buildingIds, CancellationToken cancellationToken)
        {
            var filter = Builders<Floor>.Filter;
            var query = filter.In(x => x.BuildingId, buildingIds);
            return (await Collection.FindAsync(query, cancellationToken: cancellationToken)).ToList();
        }
    }
}
