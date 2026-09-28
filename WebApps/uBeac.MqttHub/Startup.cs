using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet.AspNetCore;
using MQTTnet.Server;
using System;
using uBeac.HttpMqttCommon.Repositories;
using uBeac.HttpMqttCommon.Repositories.MongoDB;
using uBeac.HttpMqttCommon.Services;
using uBeac.Models;
using uBeac.MqttHub.Producers;
using uBeac.Repositories.Extensions;
using uBeac.Repositories.MongoDB;

namespace uBeac.MqttHub
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo<MainDatabase>("MongoDBConnection");
            services.AddMongoDBLog();
            services.AddRabbitMQClient<MqttHubProducer>("MqttHubProducer");
            services.AddSingleton<TeamService>();
            services.AddSingleton<GatewayService>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Team>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Gateway>();
            services.AddSingleton<IHubRepository, HubRepository>();
            services.AddSingleton<IHubService, HubService>();
            services.AddMqttLocalServer(Configuration);
        }

        public void Configure(IApplicationBuilder app)
        {
            // MongoDB change stream tracking activation
            app.UseStartupService<TeamService>();
            app.UseStartupService<GatewayService>();

            // the 2 below lines are to provide index.html 
            // which shows if the service is up and running
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseMqttEndpoint().UseMqttServer(cnf =>
            {
                var opt = app.ApplicationServices.GetService<IMqttServerClientDisconnectedHandler>();
                cnf.UseClientDisconnectedHandler(async (x) => await opt.HandleClientDisconnectedAsync(x));
            });

        }
    }
}
