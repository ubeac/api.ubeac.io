using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using uBeac.DeviceProcessor.Services;
using uBeac.Hosted.Service;
using uBeac.Messaging;
using uBeac.Messaging.RabbitMQ;
using uBeac.Models;

namespace uBeac.DeviceProcessor
{
    public class DeviceConsumer : Consumer
    {
        private readonly IService _service;
        private readonly PerformanceMonitor _performanceMonitor;

        public DeviceConsumer(MessagingClientOptions<DeviceConsumer> messagingClientOptions,
                            ILogger<DeviceConsumer> logger, IService service,
                            PerformanceMonitor performanceMonitor) : base(messagingClientOptions, logger)
        {
            _service = service;
            _performanceMonitor = performanceMonitor;
            Init();

            _performanceMonitor.Start();
        }

        public override async Task Handler(object sender, DeliverEventArgs deliverEventArgs)
        {
            _performanceMonitor.Increment();

            // deserialize Gateway data
            var gatewayData = deliverEventArgs.Get<GatewayData>();

            if (gatewayData.Devices.Count > 0)
                await _service.UpdateDeviceSummary(gatewayData);

        }        
    }
}
