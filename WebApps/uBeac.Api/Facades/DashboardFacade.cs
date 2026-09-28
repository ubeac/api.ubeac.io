using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Services;
using uBeac.Models;
using uBeac.Security;

namespace uBeac.Api.Facades
{
    public interface IDashboardFacade : IBaseEntityFacade<Dashboard>
    {
        Task<ResultSet<List<Widget>>> UpdateWidgetsAsync(Guid dashboardId, List<Widget> widgets, CancellationToken cancellationToken = default);
    }

    public class DashboardFacade : BaseEntityFacade<Dashboard>, IDashboardFacade
    {
        private readonly IDashboardService _dashboardService;
        private readonly IWidgetService _widgetService;

        public DashboardFacade(IDashboardService dashboardService, ISecurityContext securityContext, IWidgetService widgetService) : base(dashboardService, securityContext)
        {
            _dashboardService = dashboardService;
            _widgetService = widgetService;
        }

        public async Task<ResultSet<List<Widget>>> UpdateWidgetsAsync(Guid dashboardId, List<Widget> widgets, CancellationToken cancellationToken = default)
        {            
            var dashboard = (await base.GetByIdAsync(dashboardId, cancellationToken)).Data;

            if (dashboard is null)
                return new ResultSet<List<Widget>>(new List<Widget>());

            if (!SecurityContext.HasAccess<Team>(AccessLevels.Admin, dashboard.TeamId))
                return new ResultSet<List<Widget>>(ResponseCodes.UnAuthorized);

            foreach (var widget in widgets)
            {
                widget.CreateBy = SecurityContext.Identity.UserId;
                widget.UpdateBy = SecurityContext.Identity.UserId;
            }

            return new ResultSet<List<Widget>>(await _widgetService.UpdateWidgetsAsync(dashboardId, widgets, cancellationToken));

        }
    }
}