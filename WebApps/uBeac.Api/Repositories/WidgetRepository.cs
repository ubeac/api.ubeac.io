using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using uBeac.Models;
using System;
using MongoDB.Driver;
using System.Linq;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface IWidgetRepository: IBaseEntityRepository<Widget>
    {
        Task<List<Widget>> GetByDashboardIdsAsync(IEnumerable<Guid> dashboardIds, CancellationToken cancellationToken);
        Task<bool> DeleteByDashboardIdAsync(Guid dashboardId, CancellationToken cancellationToken);
        Task<List<Widget>> InsertManyAsync(List<Widget> widgets, CancellationToken cancellationToken);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class WidgetRepository : BaseEntityRepository<Widget>, IWidgetRepository
    {
        public WidgetRepository(MainDatabase database) : base(database)
        {
        }

        public async Task<bool> DeleteByDashboardIdAsync(Guid dashboardId, CancellationToken cancellationToken)
        {
            var query = Builders<Widget>.Filter.Eq(x => x.DashboardId, dashboardId);
            var result = await Collection.WithWriteConcern(WriteConcern.Unacknowledged).DeleteManyAsync(query, cancellationToken: cancellationToken);
            return result.IsAcknowledged;
        }

        public async Task<List<Widget>> GetByDashboardIdsAsync(IEnumerable<Guid> dashboardIds, CancellationToken cancellationToken)
        {
            var query = Builders<Widget>.Filter.In(x => x.DashboardId, dashboardIds);
            return (await Collection.FindAsync(query, cancellationToken: cancellationToken)).ToList();
        }

        public async Task<List<Widget>> InsertManyAsync(List<Widget> widgets, CancellationToken cancellationToken)
        {            
            await Collection.InsertManyAsync(widgets, new InsertManyOptions { IsOrdered = false });
            return widgets;
        }
    }
}