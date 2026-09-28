using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using uBeac.Models;
using uBeac.Repositories.Extensions;
using uBeac.Repositories.MongoDB;
using uBeac.SocketApi.Repositories;
using uBeac.SocketApi.Services;
using uBeac.WebSocket;

namespace uBeac.SocketApi
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiDocumantation(Configuration);

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration.GetValue<string>("IdsrvAuthority");
                    options.ApiName = "uBeacSocketApi";
                    options.RequireHttpsMetadata = true;
                    options.TokenRetriever = new Func<HttpRequest, string>(req =>
                    {
                        var fromHeader = TokenRetrieval.FromAuthorizationHeader();
                        var fromQuery = TokenRetrieval.FromQueryString();
                        return fromHeader(req) ?? fromQuery(req);
                    });
                });

            services.AddCustomCors(Configuration);

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            //services.AddSingleton<MongoDatabaseFactory>();
            services.AddMongo<MainDatabase>("uBeacDBConnection");
            services.AddMongo<DeviceSummaryDatabase>("uBeacDeviceSummaryDBConnection");

            services.AddHttpContextAccessor();
            services.AddAccessControl();

            services.AddMongoDBLog();

            services.AddSingleton<ISocket, BaseSocket>();
            //services.AddSingleton<BaseHub>();
            services.AddScoped<BaseHub>();

            services.AddRabbitMQClient<SocketConsumer>("GatewayDataWebSocket");

            
            services.AddSingleton<BaseEntityTrackerService<Team>>();
            services.AddSingleton<BaseEntityTrackerService<Building>>();
            services.AddSingleton<BaseEntityTrackerService<Floor>>();
            services.AddSingleton<BaseEntityTrackerService<Device>>();
            services.AddSingleton<BaseEntityTrackerService<Sensor>>();
            services.AddSingleton<BaseEntityTrackerService<Dashboard>>();
            services.AddSingleton<BaseEntityTrackerService<Widget>>();
            services.AddSingleton<GatewayRequestService>();
            services.AddSingleton<DeviceRequestService>();

            services.AddMongoChangeTracker<MainDatabase, Guid, Team>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Gateway>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Building>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Floor>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Device>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Sensor>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Dashboard>();
            services.AddMongoChangeTracker<MainDatabase, Guid, Widget>();
            services.AddMongoChangeTracker<DeviceSummaryDatabase, Guid, DeviceSummary>();

            services.AddSingleton<IRepository<Team>, Repository<Team>>();
            services.AddSingleton<IRepository<Gateway>, Repository<Gateway>>();
            services.AddSingleton<IRepository<Building>, Repository<Building>>();
            services.AddSingleton<IRepository<Floor>, Repository<Floor>>();
            services.AddSingleton<IRepository<Device>, Repository<Device>>();
            services.AddSingleton<IRepository<Sensor>, Repository<Sensor>>();
            services.AddSingleton<IRepository<Dashboard>, Repository<Dashboard>>();
            services.AddSingleton<IRepository<Widget>, Repository<Widget>>();

            // todo: do we need this
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStartupService<BaseEntityTrackerService<Team>>();
            app.UseStartupService<BaseEntityTrackerService<Building>>();
            app.UseStartupService<BaseEntityTrackerService<Floor>>();
            app.UseStartupService<BaseEntityTrackerService<Device>>();
            app.UseStartupService<BaseEntityTrackerService<Sensor>>();
            app.UseStartupService<BaseEntityTrackerService<Dashboard>>();
            app.UseStartupService<BaseEntityTrackerService<Widget>>();
            app.UseStartupService<GatewayRequestService>();
            app.UseStartupService<DeviceRequestService>();
            

            app.UseRabbitMq<SocketConsumer>();

            //app.UseCors(o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            if (Environment.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseCustomCors();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseSignalR(routes =>
            {
                routes.MapHub<BaseHub>("/socket");
            });
            app.UseApiDocumantation(Environment);
            // todo: do we need this
            app.UseMvcWithDefaultRoute();
        }

    }
}
