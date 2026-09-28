using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using uBeac.HttpMqttCommon.Models;
using uBeac.HttpMqttCommon.Services;
using uBeac.Models;

namespace uBeac.HttpHub.Middlewares
{
    public class GatewayValidatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHubService _hubService;

        public GatewayValidatorMiddleware(RequestDelegate next, IHubService hubService)
        {
            _next = next;
            _hubService = hubService;
        }

        public async Task Invoke(HttpContext context)
        {
            var _namespace = context.Items[Constants.TEAM_NAMESPACE].ToString();
            var gatewayUrl = context.Items[Constants.GATEWAY_URL].ToString();
            var teamsModel = _hubService.Teams;
            if (!teamsModel.TryGetByNamespace(_namespace, out TeamModel teamModel))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            if (!teamModel.Gateways.TryGetByUrl(gatewayUrl, out Gateway gateway))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Items[Constants.TEAM_ID] = teamModel.Id;
            context.Items[Constants.GATEWAY] = gateway;
            
            await _next(context);

        }
    }
}
