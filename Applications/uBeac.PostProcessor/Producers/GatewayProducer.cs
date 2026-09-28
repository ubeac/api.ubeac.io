using System.Threading.Tasks;
using uBeac.Messaging.RabbitMQ;
using uBeac.Models;

namespace uBeac.PostProcessor.Producers
{
    public class GatewayProducer : Producer
    {
        public GatewayProducer(MessagingClientOptions<GatewayProducer> messagingClientOptions) : base(messagingClientOptions)
        {
        }
        public async Task Send(GatewayData gatewayData)
        {
            await Send<GatewayData>(gatewayData);
        }
    }
}
