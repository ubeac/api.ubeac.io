using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace uBeac.HttpHub.Middlewares
{
    public class NamespaceValidatorMiddleware
    {
        private readonly RequestDelegate _next;
        public NamespaceValidatorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var _request = context.Request;
            var hostSegments = _request.Host.Host.Split('.');

            // Note: checking host name to be 4 parts extactly: abcdef.hub.ubeac.io/MyGatewayUrl/xxx/yyy/zzz
            if (hostSegments.Length != 4)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }            

            if (!_request.Path.HasValue)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var paths = _request.Path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries);

            if (paths.Length == 0)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Items[Constants.TEAM_NAMESPACE] = hostSegments[0].ToLower();
            context.Items[Constants.GATEWAY_URL] = paths[0];

            await _next(context);

        }
    }
}
