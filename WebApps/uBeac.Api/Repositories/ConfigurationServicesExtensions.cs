using Microsoft.Extensions.DependencyInjection;

namespace uBeac.Api.Repositories
{
    public static class ConfigurationServicesExtensions
    {
        public static IServiceCollection AddRepository<TService, TImplementation>(this IServiceCollection services) 
            where TService : class
            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>();
            return services;
        }

    }
}
