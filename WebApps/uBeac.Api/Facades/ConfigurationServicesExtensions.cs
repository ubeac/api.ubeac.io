using Microsoft.Extensions.DependencyInjection;

namespace uBeac.Api.Facades
{
    public static class ConfigurationServicesExtensions
    {
        public static IServiceCollection AddFacade<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddScoped<TService, TImplementation>();
            return services;
        }
    }
}
