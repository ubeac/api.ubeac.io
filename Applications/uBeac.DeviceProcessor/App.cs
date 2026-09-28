using Microsoft.Extensions.DependencyInjection;
using System;
using uBeac.DeviceProcessor.Repositories;
using uBeac.DeviceProcessor.Repositories.MongoDB;
using uBeac.DeviceProcessor.Services;
using uBeac.Hosted.Service;
using uBeac.Repositories.MongoDB;

namespace uBeac.DeviceProcessor
{
    public class App : BaseApp
    {

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddRabbitMQClient<DeviceConsumer>("DeviceConsumer");

            // defining MongoClient
            services.AddMongo<DeviceSummaryDatabase>("uBeacDeviceSummaryDBConnection");
            services.AddSingleton<IService, Service>();
            services.AddSingleton<IRepository, Repository>();
        }

        public override void Configure(IServiceProvider serviceProvider)
        {
            serviceProvider.UseRabbitMq<DeviceConsumer>();
        }

    }
}
