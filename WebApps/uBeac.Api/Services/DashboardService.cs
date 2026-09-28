using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Repositories;
using uBeac.Models;

namespace uBeac.Api.Services
{
    public interface IDashboardService : IBaseEntityService<Dashboard>
    {        
    }

    public class DashboardService : BaseEntityService<Dashboard>, IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IWidgetRepository _widgetRepository;

        public DashboardService(IDashboardRepository dashboardRepository, IWidgetRepository widgetRepository) : base(dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
            _widgetRepository = widgetRepository;
        }

        public override async Task<List<Dashboard>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            var dashboards = await _dashboardRepository.GetByTeamIdAsync(teamId, cancellationToken);
            var widgets = await _widgetRepository.GetByDashboardIdsAsync(dashboards.Select(x => x.Id), cancellationToken);
            var dashboardsDict = dashboards.ToDictionary(x => x.Id, y => y);
            foreach (var widget in widgets)
            {
                if (dashboardsDict.ContainsKey(widget.DashboardId))
                    dashboardsDict[widget.DashboardId].Widgets.Add(widget);
            }
            return dashboards;
        }

        public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Deleting related widgets
            await _widgetRepository.DeleteByDashboardIdAsync(id, cancellationToken);

            // Deleting dashboard
            return await base.DeleteAsync(id, cancellationToken);
        }                
    }
}
