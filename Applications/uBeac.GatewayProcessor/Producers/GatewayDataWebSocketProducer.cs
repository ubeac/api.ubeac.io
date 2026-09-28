using System.Threading.Tasks;
using uBeac.Messaging.RabbitMQ;
using uBeac.Models;

namespace uBeac.GatewayProcessor.Producers
{
    public class GatewayDataWebSocketProducer : Producer
    {
        public GatewayDataWebSocketProducer(MessagingClientOptions<GatewayDataWebSocketProducer> messagingClientOptions) : base(messagingClientOptions)
        {
        }
        public async Task Send(GatewayData gatewayData)
        {
            await Send<GatewayData>(gatewayData);
        }
    }
}
