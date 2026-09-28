using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using uBeac.Hosted.Service;
using uBeac.IoT.Processing;
using uBeac.Messaging;
using uBeac.Messaging.RabbitMQ;
using uBeac.Models;
using uBeac.Processor.Producers;

namespace uBeac.Processor
{
    public class ProcessorConsumer : Consumer
    {
        private readonly IProcessorPool _processorPool;
        private readonly PostProcessorProducer _postProcessorProducer;
        private readonly PerformanceMonitor _performanceMonitor;
        
        public ProcessorConsumer(MessagingClientOptions<ProcessorConsumer> messagingClientOptions,
                                IProcessorPool processorPool, PostProcessorProducer postProcessorProducer, 
                                ILogger<ProcessorConsumer> logger,
                                PerformanceMonitor performanceMonitor) : base(messagingClientOptions, logger)
        {
            _postProcessorProducer = postProcessorProducer;
            _processorPool = processorPool;
            _performanceMonitor = performanceMonitor;

            // todo: why we need init in all consumers
            Init();

            _performanceMonitor.Start();
        }

        public override async Task Handler(object sender, DeliverEventArgs deliverEventArgs)
        {
            _performanceMonitor.Increment();

            var gatewayData = deliverEventArgs.Get<GatewayData>();

            // getting compiled processor for the firmware
            IProcessor processor = _processorPool.GetProcessor(gatewayData.FirmwareId.ToString());

            // processing the request body for the Gateway
            processor.Process(gatewayData);

            SetDefaultProperties(gatewayData);

            await _postProcessorProducer.Send(gatewayData);
        }

        private void SetDefaultProperties(GatewayData gatewayData)
        {
            foreach (var device in gatewayData.RawDevices)
            {
                device.GatewayId = gatewayData.GatewayId;

                foreach (var sensor in device.Sensors)
                {
                    sensor.DateTime = device.DateTime;
                }
            }
        }
        
    }
}
