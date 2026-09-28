using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using System;
using uBeac.Hosted.Service;
using uBeac.Models;
using uBeac.Repositories.MongoDB;
using uBeac.SensorProcessor.Repositories;
using uBeac.SensorProcessor.Repositories.MongoDB;
using uBeac.SensorProcessor.Services;

namespace uBeac.SensorProcessor
{
    public class App : BaseApp
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo<SensorDataDatabase>("uBeacSensorDataDBConnection");
            services.AddSingleton<IService, Service>();
            services.AddSingleton<IRepository, Repository>();
            services.AddRabbitMQClient<SensorConsumer>("SensorConsumer");

            BsonClassMap.RegisterClassMap<SensorData>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(c => c.Data).SetSerializer(new MongoGeoJsonSerializer());
            });
        }

        public override void Configure(IServiceProvider serviceProvider)
        {
            serviceProvider.UseRabbitMq<SensorConsumer>();
        }

    }
}
