using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using uBeac.HttpHub.Caches;

namespace uBeac.HttpHub.Middlewares
{
    public class RequestThrottlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ThrottleCache _cache;
        public RequestThrottlingMiddleware(RequestDelegate next, ThrottleCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context)
        {
            var _namespace = context.Items[Constants.TEAM_NAMESPACE];
            var gatewayUrl = context.Items[Constants.GATEWAY_URL];

            // Note: if these namespace and gatewayUrl exist in cache it means the last request was less than 1s ago and we should reject it
            if (!_cache.TryGetValue((_namespace, gatewayUrl), out bool result))
            {
                _cache.Set((_namespace, gatewayUrl), false);
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return;
            }
        }
    }
}
