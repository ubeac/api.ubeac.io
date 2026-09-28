using Microsoft.AspNetCore.Builder;

namespace uBeac.Api.Middlewares
{
    public static class TokenAuthenticationExtensions
    {
        public static IApplicationBuilder UseTokenAuthentication(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
