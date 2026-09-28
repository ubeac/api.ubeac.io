using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace uBeac.Hosted.Service
{
    public class ServiceStarter
    {
        public static void Run<THostedService>(string[] args) where THostedService : class, IHostedService
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<THostedService>();
                });

            if (isService)
            {
                builder.RunAsServiceAsync().Wait();
            }
            else
            {
                builder.RunConsoleAsync().Wait();
            }
        }
    }
}
