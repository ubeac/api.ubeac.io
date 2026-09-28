using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using uBeac.Hosted.Service;
using uBeac.Messaging;
using uBeac.Messaging.RabbitMQ;
using uBeac.Models;
using uBeac.SensorProcessor.Services;

namespace uBeac.SensorProcessor
{
    public class SensorConsumer : Consumer
    {
        private readonly IService _service;
        private readonly PerformanceMonitor _performanceMonitor;

        public SensorConsumer(MessagingClientOptions<SensorConsumer> messagingClientOptions, ILogger<SensorConsumer> logger, 
            IService service, PerformanceMonitor performanceMonitor) : base(messagingClientOptions, logger)
        {
            _service = service;
            _performanceMonitor = performanceMonitor;
            // todo: why we need init in all consumers
            Init();

            _performanceMonitor.Start();
        }

        public override async Task Handler(object sender, DeliverEventArgs deliverEventArgs)
        {
            _performanceMonitor.Increment();

            // deserialize Gateway data
            var gatewayData = deliverEventArgs.Get<GatewayData>();

            // update DB
            if (gatewayData.Devices.Count > 0)
                await _service.InsertSensorData(gatewayData);
        }
    }
}
