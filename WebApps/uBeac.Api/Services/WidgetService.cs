using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Repositories;
using uBeac.Models;

namespace uBeac.Api.Services
{
    public interface IWidgetService : IBaseEntityService<Widget>
    {
        Task<List<Widget>> UpdateWidgetsAsync(Guid dashboardId, List<Widget> widgets, CancellationToken cancellationToken = default);
    }

    public class WidgetService : BaseEntityService<Widget>, IWidgetService
    {
        private readonly IWidgetRepository _widgetRepository;

        public WidgetService(IWidgetRepository widgetRepository) : base(widgetRepository)
        {
            _widgetRepository = widgetRepository;
        }

        public async Task<List<Widget>> UpdateWidgetsAsync(Guid dashboardId, List<Widget> widgets, CancellationToken cancellationToken = default)
        {
            var dateTime = DateTime.UtcNow;
            foreach (var widget in widgets)
            {
                widget.Id = Guid.NewGuid();
                widget.CreateDate = dateTime;
                widget.UpdateDate = dateTime;
            }

            await _widgetRepository.DeleteByDashboardIdAsync(dashboardId, cancellationToken);

            if (widgets.Count == 0)
                return widgets;
            else
                return await _widgetRepository.InsertManyAsync(widgets, cancellationToken);
        }
    }
}
