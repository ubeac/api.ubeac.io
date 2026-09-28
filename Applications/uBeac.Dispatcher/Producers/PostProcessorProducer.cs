using System.Threading.Tasks;
using uBeac.Models;

namespace uBeac.Messaging.RabbitMQ
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
