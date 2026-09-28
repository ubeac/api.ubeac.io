/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using uBeac.Messaging;
using uBeac.Messaging.RabbitMQ;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMQExtensions
    {
        public static IServiceCollection AddRabbitMQClient<TMessagingClient>(this IServiceCollection services, string clientName) where TMessagingClient : class, IMessagingClient
        {
            services.TryAddSingleton(provider =>
            {
                var configuration = provider.GetService<IConfiguration>();
                var settings = configuration.GetSection("RabbitMQ").Get<Dictionary<string, MessagingClientSettings>>();

                foreach (var item in settings.Values)
                {
                    item.ConnectionString = configuration.GetConnectionString(item.ConnectionStringName);
                }
                return settings;
            });

            services.AddSingleton(provider =>
            {
                var queueSettings = provider.GetService<Dictionary<string, MessagingClientSettings>>()[clientName];
                return new MessagingClientOptions<TMessagingClient>(queueSettings);
            });

            services.AddSingleton(provider =>
            {
                var queueSettings = provider.GetService<Dictionary<string, MessagingClientSettings>>()[clientName];
                return new MessagingClientOptions(queueSettings);
            });

            services.AddSingleton<TMessagingClient>();

            return services;
        }

        public static IApplicationBuilder UseRabbitMq<TMessagingClient>(this IApplicationBuilder app)
        {
            var instance = app.ApplicationServices.GetService<TMessagingClient>();
            return app;
        }

        public static IServiceProvider UseRabbitMq<TMessagingClient>(this IServiceProvider service)
        {
            var instance = service.GetService<TMessagingClient>();
            return service;
        }
    }
}
