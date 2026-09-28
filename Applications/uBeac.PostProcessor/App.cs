using Microsoft.Extensions.DependencyInjection;
using System;
using uBeac.Hosted.Service;
using uBeac.Models;
using uBeac.PostProcessor.ChangeTrackers;
using uBeac.PostProcessor.Models;
using uBeac.PostProcessor.Producers;
using uBeac.PostProcessor.Repositories;
using uBeac.PostProcessor.Repositories.MongoDB;
using uBeac.PostProcessor.ServiceProcessors;
using uBeac.PostProcessor.Services;
using uBeac.Repositories.Extensions;
using uBeac.Repositories.MongoDB;

namespace uBeac.PostProcessor
{
    public class App : BaseApp
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddRabbitMQClient<PostProcessorConsumer>("PostProcessorConsumer");
            services.AddRabbitMQClient<GatewayProducer>("GatewayProducer");
            services.AddRabbitMQClient<SensorProducer>("SensorProducer");
            services.AddRabbitMQClient<DeviceProducer>("DeviceProducer");

            // defining MongoClient
            services.AddMongo<MainDatabase>("uBeacDBConnection");
            services.AddSingleton<NewValidDeviceProcessor>();
            services.AddSingleton<ExistingDeviceProcessor>();
            services.AddSingleton<DeviceDataExtractor>();
            services.AddSingleton<TeamsModel>();
            services.AddSingleton<Service>();
            services.AddSingleton<TeamChangeTrackerService>();
            services.AddSingleton<DeviceChangeTrackerService>();
            services.AddSingleton<SensorChangeTrackerService>();
            services.AddSingleton<IRepository, Repository>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Team>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Device>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Sensor>();
        }

        public override void Configure(IServiceProvider serviceProvider)
        {
            // MongoDB change stream tracking activation
            serviceProvider.UseStartupService<TeamChangeTrackerService>();
            serviceProvider.UseStartupService<DeviceChangeTrackerService>();
            serviceProvider.UseStartupService<SensorChangeTrackerService>();

            serviceProvider.GetService<Service>();
            
            serviceProvider.UseRabbitMq<PostProcessorConsumer>();
        }

    }
}
