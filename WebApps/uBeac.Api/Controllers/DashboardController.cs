using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uBeac.Api.Facades;
using uBeac.Api.InputModels;
using uBeac.Models;

namespace uBeac.Api.Controllers
{
    [SwaggerTag(DashboardConstants.DESCRIPTION)]
    public class DashboardController : BaseEntityController<IDashboardFacade, Dashboard, DashboardInputModelAdd, DashboardInputModelUpdate>
    {
        private readonly IDashboardFacade _dashboardFacade;

        public DashboardController(IDashboardFacade dashboardFacade) : base(dashboardFacade)
        {
            _dashboardFacade = dashboardFacade;
        }

        [HttpPost]
        [ProducesJson]
        [SwaggerOperation(Summary = DashboardConstants.UPDATE_WIDGETS_SUMMARY, Description = DashboardConstants.UPDATE_WIDGETS_DESCRIPTION)]
        public async Task<ResultSet<List<Widget>>> UpdateWidgets([FromBody] WidgetInputModel inputModel)
        {
            var widgetList = new List<Widget>();

            foreach (var widgetModel in inputModel.Widgets)
            {
                var widget = Mapping.Mapper.Map<Widget>(widgetModel);
                widget.DashboardId = inputModel.DashboardId;
                widget.TeamId = inputModel.TeamId;
                widgetList.Add(widget);
            }

            return await _dashboardFacade.UpdateWidgetsAsync(inputModel.DashboardId, widgetList);
        }

        [SwaggerOperation(Summary = DashboardConstants.DELETE_SUMMARY, Description = DashboardConstants.DELETE_DESCRIPTION)]
        public override async Task<ResultSet<bool>> Remove(Guid id)
        {
            return await base.Remove(id);
        }
    }
}
