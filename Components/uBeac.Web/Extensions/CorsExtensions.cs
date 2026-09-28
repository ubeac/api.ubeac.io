using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using uBeac;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CorsExtensions
    {
        public const string CorsName = "AppConfigCors";

        public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
        {

            var appConfig = configuration.GetSection("AppConfig").Get<AppConfig>();

            services.AddCors(options =>
            {
                options.AddPolicy(CorsName, policy =>
                {
                    foreach (string item in appConfig.CorsOrigins)
                    {
                        policy.WithOrigins(item).AllowAnyHeader().AllowAnyMethod();
                    }
                });
            });
            
            return services;
        }

        public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app)
        {
            app.UseCors(CorsName);
            return app;
        }

    }
    
}
