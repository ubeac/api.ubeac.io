using Microsoft.AspNetCore.Builder;
using System;

namespace uBeac.Repositories.Extensions
{
    public static class ChangeTrackerServiceExtensions
    {
        public static IApplicationBuilder UseStartupService<TService>(this IApplicationBuilder app) where TService : IChangeTrackerStartupService
        {
            var instance = (IChangeTrackerStartupService)app.ApplicationServices.GetService(typeof(TService));
            instance.Run();
            return app;
        }

        public static IServiceProvider UseStartupService<TService>(this IServiceProvider serviceProvider) where TService : IChangeTrackerStartupService
        {
            var instance = (IChangeTrackerStartupService)serviceProvider.GetService(typeof(TService));
            instance.Run();
            return serviceProvider;
        }

    }
}
