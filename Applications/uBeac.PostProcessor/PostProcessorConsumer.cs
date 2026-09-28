using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using uBeac.Hosted.Service;
using uBeac.Messaging;
using uBeac.Messaging.RabbitMQ;
using uBeac.Models;
using uBeac.PostProcessor.Producers;
using uBeac.PostProcessor.ServiceProcessors;

namespace uBeac.PostProcessor
{
    public class PostProcessorConsumer : Consumer
    {
        private readonly ExistingDeviceProcessor _existingDeviceProcessor;
        private readonly NewValidDeviceProcessor _newValidDeviceProcessor;
        private readonly DeviceDataExtractor _deviceDataExtractor;
        private readonly GatewayProducer _gatewayProducer;
        private readonly SensorProducer _sensorProducer;
        private readonly DeviceProducer _deviceProducer;
        private readonly PerformanceMonitor _performanceMonitor;

        public PostProcessorConsumer(MessagingClientOptions<PostProcessorConsumer> messagingClientOptions,
                                    GatewayProducer gatewayProducer, ILogger<PostProcessorConsumer> logger,
                                    SensorProducer sensorProducer, DeviceProducer deviceProducer,
                                    ExistingDeviceProcessor existingDeviceProcessor,
                                    NewValidDeviceProcessor newValidDeviceProcessor,
                                    DeviceDataExtractor deviceDataExtractor,
                                    PerformanceMonitor performanceMonitor) : base(messagingClientOptions, logger)
        {
            _gatewayProducer = gatewayProducer;
            _sensorProducer = sensorProducer;
            _deviceProducer = deviceProducer;
            _performanceMonitor = performanceMonitor;
            _existingDeviceProcessor = existingDeviceProcessor;
            _newValidDeviceProcessor = newValidDeviceProcessor;
            _deviceDataExtractor = deviceDataExtractor;

            Init();

            _performanceMonitor.Start();
        }

        public override async Task Handler(object sender, DeliverEventArgs deliverEventArgs)
        {
            _performanceMonitor.Increment();

            // deserialize Gateway data
            var gatewayData = deliverEventArgs.Get<GatewayData>();

            if (gatewayData.RawDevices != null && gatewayData.RawDevices.Count > 0)
            {
                foreach (var deviceRawData in gatewayData.RawDevices)
                {
                    await _existingDeviceProcessor.ProcessAsync(gatewayData, deviceRawData);
                    await _newValidDeviceProcessor.ProcessAsync(gatewayData, deviceRawData);                    
                    await _deviceDataExtractor.ProcessAsync(gatewayData, deviceRawData);
                }
            }

            var tasks = new List<Task>
            {
                _gatewayProducer.Send(gatewayData),
                _sensorProducer.Send(gatewayData),
                _deviceProducer.Send(gatewayData)
            };

            await Task.WhenAll(tasks);
        }

    }
}
