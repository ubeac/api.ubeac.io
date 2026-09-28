using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace uBeac.Hosted.Service
{
    public abstract class BaseApp : IHostedService, IDisposable
    {
        public IConfiguration Configuration { get; set; }

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            var services = new ServiceCollection();

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            Configuration = new ConfigurationBuilder()
              .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
              .AddJsonFile("appsettings.json", optional: false)
              .AddJsonFile($"appsettings.{environmentName}.json", optional: false, reloadOnChange: true)
              .Build();

            // this line is used for legacy mongodb log, test and remove this safely
            // this line is for incompatibility in MongoDB driver with Serilog
            // DO NOT remove this line
            //MongoDB.Bson.BsonDefaults.GuidRepresentation = MongoDB.Bson.GuidRepresentation.Standard;

            services.AddSingleton(Configuration);
            services.AddMongoDBLog();

            services.AddSingleton(x => new PerformanceMonitor() { Active = true, MilliSeconds = 1000 });

            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            Configure(serviceProvider);

            Log.Logger.Warning("{ApplicationName} started on: {DateTime}", AppDomain.CurrentDomain.FriendlyName, DateTime.UtcNow);

            return Task.CompletedTask;
        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Logger.Warning("{ApplicationName} stopped on: {DateTime}", AppDomain.CurrentDomain.FriendlyName, DateTime.UtcNow);

            return Task.CompletedTask;
        }

        public abstract void ConfigureServices(IServiceCollection services);

        public abstract void Configure(IServiceProvider serviceProvider);

        public virtual void Dispose()
        {
        }

    }
}
