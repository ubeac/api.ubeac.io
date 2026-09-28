using Microsoft.Extensions.DependencyInjection;
using System;
using uBeac.Hosted.Service;
using uBeac.Messaging.RabbitMQ;

namespace uBeac.Dispatcher
{
    public class App : BaseApp
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddRabbitMQClient<DataConsumer>("DispatcherConsumer");
            services.AddRabbitMQClient<CSharpProcessorProducer>("ProcessorProducer");
            services.AddRabbitMQClient<JavaScriptProcessorProducer>("JavaScriptProcessorProducer");
            services.AddRabbitMQClient<PostProcessorProducer>("PostProcessorProducer");
        }

        public override void Configure(IServiceProvider serviceProvider)
        {
            serviceProvider.UseRabbitMq<DataConsumer>();
        }

    }
}
