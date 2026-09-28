using Microsoft.AspNetCore.Builder;
using uBeac.Notifications;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Extensions
    {
        public static IServiceCollection AddRabbitMQNotifier(this IServiceCollection services, string clientName)
        {
            services.AddRabbitMQClient<ChangeNotifier>("SocketNotification");
            services.AddSingleton<IChangeNotifier, ChangeNotifier>();

            return services;
        }

        public static IApplicationBuilder UseRabbitMQNotifier(this IApplicationBuilder app)
        {
            Notification.Instance = app.ApplicationServices.GetService<IChangeNotifier>();
            return app;
        }
    }
}
