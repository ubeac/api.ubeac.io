using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using uBeac.HttpHub.Caches;
using uBeac.HttpHub.Controllers;
using uBeac.HttpHub.Middlewares;
using uBeac.HttpHub.Producers;
using uBeac.HttpMqttCommon.Repositories;
using uBeac.HttpMqttCommon.Repositories.MongoDB;
using uBeac.HttpMqttCommon.Services;
using uBeac.Models;
using uBeac.Repositories.Extensions;
using uBeac.Repositories.MongoDB;
using uBeac.Web.Middlewares;

namespace uBeac.HttpHub
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // MongoDB
            services.AddMongo<MainDatabase>("MongoDBConnection");
            services.AddMongoDBLog();

            // RabbitMQ
            services.AddRabbitMQClient<HubProducer>("HttpHubProducer");

            // Custom caches
            services.AddSingleton<ThrottleCache>();

            // MongoDB change stream tracking 
            services.AddSingleton<TeamService>();
            services.AddSingleton<GatewayService>();

            services.AddSingleton<IHubRepository, HubRepository>();
            services.AddSingleton<IHubService, HubService>();
            services.AddTransient<HttpController>();

            services.AddMongoChangeTracker<MainDatabase, Guid, Team>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Gateway>();

            // built-in dependencies for MVC
            services.AddHttpContextAccessor();

            // todo: think to remove this line
            services.AddMvcCore();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // MongoDB change stream tracking activation
            app.UseStartupService<TeamService>();
            app.UseStartupService<GatewayService>();

            // the 2 below lines are to provide index.html 
            // which shows if the service is up and running
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // logging, exception logging
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();

            // custom middlewares for validation, authentication, throttling
            // ********* ORDER OF EXECUTION IS VERY IMPORTANT !********
            app.UseMiddleware<NamespaceValidatorMiddleware>();
            app.UseMiddleware<RequestThrottlingMiddleware>();
            app.UseMiddleware<GatewayValidatorMiddleware>();
            app.UseMiddleware<GatewaySecurityMiddleware>();
                        
            // todo: think to remove thiis line
            app.UseMvc();
        }

    }
}
