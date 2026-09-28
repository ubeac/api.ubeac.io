using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using uBeac.GatewayProcessor.Producers;
using uBeac.GatewayProcessor.Services;
using uBeac.Hosted.Service;
using uBeac.Messaging;
using uBeac.Messaging.RabbitMQ;
using uBeac.Models;

namespace uBeac.GatewayProcessor
{
    public class GatewayConsumer : Consumer
    {
        private readonly IService _service;
        private readonly GatewayDataWebSocketProducer _gatewayDataWebSocketProducer;
        private readonly PerformanceMonitor _performanceMonitor;

        public GatewayConsumer(MessagingClientOptions<GatewayConsumer> messagingClientOptions,
                                ILogger<GatewayConsumer> logger,
                                IService service,
                                GatewayDataWebSocketProducer gatewayDataWebSocketProducer,
                                PerformanceMonitor performanceMonitor) : base(messagingClientOptions, logger)
        {
            _service = service;
            _gatewayDataWebSocketProducer = gatewayDataWebSocketProducer;
            _performanceMonitor = performanceMonitor;
            // todo: why we need init in all consumers
            Init();

            _performanceMonitor.Start();
        }

        public override async Task Handler(object sender, DeliverEventArgs deliverEventArgs)
        {
            _performanceMonitor.Increment();

            var gatewayData = deliverEventArgs.Get<GatewayData>();

            var tasks = new List<Task>()
            {
                // Sending GatewayData complete object to WebSocket to be shown in (Gateway Info Page)
                _gatewayDataWebSocketProducer.Send(gatewayData),
                
                // update RequestCount and LastRequestDate for the gateway (General uBeac DB)
                _service.UpdateGateway(gatewayData.GatewayId, gatewayData.DateTime),
                
                // Inserting GatewayData to its collection
                _service.InsertGatewayData(gatewayData)
            };
            
            await Task.WhenAll(tasks.ToArray());
        }
    }
}
