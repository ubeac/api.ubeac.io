using Microsoft.Extensions.DependencyInjection;

namespace uBeac.Api.Services
{
    public static class ConfigurationServicesExtensions
    {
        public static IServiceCollection AddService<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>();
            return services;
        }
    }
}
