using Microsoft.Extensions.DependencyInjection;
using System;
using uBeac.GatewayProcessor.Producers;
using uBeac.GatewayProcessor.Repositories;
using uBeac.GatewayProcessor.Repositories.MongoDB;
using uBeac.GatewayProcessor.Services;
using uBeac.Hosted.Service;
using uBeac.Repositories.MongoDB;

namespace uBeac.GatewayProcessor
{
    public class App : BaseApp
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo<MainDatabase>("uBeacDBConnection");
            services.AddMongo<GatewayDataDatabase>("uBeacGatewayDataDBConnection");
            services.AddSingleton<IService, Service>();
            services.AddSingleton<IRepository, Repository>();
            services.AddRabbitMQClient<GatewayConsumer>("GatewayConsumer");
            services.AddRabbitMQClient<GatewayDataWebSocketProducer>("GatewayDataWebSocketProducer");
        }

        public override void Configure(IServiceProvider serviceProvider)
        {
            serviceProvider.UseRabbitMq<GatewayConsumer>();
        }
    }
}
