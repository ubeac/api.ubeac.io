using System.Threading.Tasks;
using uBeac.Messaging.RabbitMQ;
using uBeac.Models;

namespace uBeac.Processor.Producers
{
    public class PostProcessorProducer : Producer
    {
        public PostProcessorProducer(MessagingClientOptions<PostProcessorProducer> messagingClientOptions) : base(messagingClientOptions)
        {
        }
        public async Task Send(GatewayData gatewayData)
        {            
            await Send<GatewayData>(gatewayData);
        }
    }
}
