using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using uBeac.Hosted.Service;
using uBeac.Messaging;
using uBeac.Messaging.RabbitMQ;
using uBeac.Models;

namespace uBeac.Dispatcher
{
    public class DataConsumer : Consumer
    {
        private readonly CSharpProcessorProducer _cSharpProcessorProducer;
        // private readonly JavaScriptProcessorProducer _javaScriptProcessorProducer;
        private readonly PostProcessorProducer _postProcessorProducer;
        private readonly PerformanceMonitor _performanceMonitor;
        
        public DataConsumer(MessagingClientOptions<DataConsumer> messagingClientOptions,
                                    ILogger<DataConsumer> logger,
                                    CSharpProcessorProducer cSharpProcessorProducer,
                                    /*JavaScriptProcessorProducer javaScriptProcessorProducer,*/
                                    PerformanceMonitor performanceMonitor,
                                    PostProcessorProducer postProcessorProducer) : base(messagingClientOptions, logger)
        {
            _cSharpProcessorProducer = cSharpProcessorProducer;
            // _javaScriptProcessorProducer = javaScriptProcessorProducer;
            _postProcessorProducer = postProcessorProducer;
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

            // todo: In case of user defined custom processor, we need to detect appropriate processor and send it to related producer
            #region Processor Detection

            //var language = 0;

            //if (gatewayData.ContainsKey("Language"))
            //{
            //    if (!int.TryParse(gatewayData["Language"].ToString(), out language))
            //        language = 0;
            //}

            //Producer producer = null;

            //// send the recieved bytes to the next queue
            //switch (language)
            //{
            //    case 0:
            //        // C# predefined processors
            //        producer = _cSharpProcessorProducer;
            //        break;

            //    case 1:
            //        // JavaScript user-defined processor codes 
            //        producer = _javaScriptProcessorProducer;
            //        break;

            //    default:
            //        break;
            //}

            #endregion

            if (string.IsNullOrEmpty(gatewayData.Body) && gatewayData.RawDevices.Count > 0)
                // Note: SingleSensorData that is recieved from url without payload
                await _postProcessorProducer.Send(gatewayData);
            else
                // sending the recieved bytes to the next queue
                await _cSharpProcessorProducer.SendBytes(deliverEventArgs.Bytes);
        }
    }
}
