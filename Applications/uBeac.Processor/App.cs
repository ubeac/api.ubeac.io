using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using uBeac.Hosted.Service;
using uBeac.IoT.Processing;
using uBeac.Processor.Producers;
using uBeac.Processor.Repositories;
using uBeac.Processor.Repositories.MongoDB;
using uBeac.Processor.Services;
using uBeac.Repositories.MongoDB;

namespace uBeac.Processor
{
    public class App : BaseApp
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddRabbitMQClient<ProcessorConsumer>("ProcessorConsumer");
            services.AddRabbitMQClient<PostProcessorProducer>("PostProcessor");

            services.AddMongo<MainDatabase>("uBeacDBConnection");
            services.AddSingleton<IService, Service>();
            services.AddSingleton<IRepository, Repository>();
            services.AddSingleton<IProcessorPool, ProcessorPool>();
        }

        public override void Configure(IServiceProvider serviceProvider)
        {
            // loading Firmwares to extract the FirmwareId and the codes which needed to be compiled
            // todo: Amir, write clean code!!!!
            var firmwares = serviceProvider.GetService<IService>().GetAllFirmwares().Result;
            serviceProvider.UserProcessorPool(firmwares.ToDictionary(p => p.Id.ToString(), q => q.Processor));

            // starting RabbitMQ
            serviceProvider.UseRabbitMq<ProcessorConsumer>();
        }
    }

}
