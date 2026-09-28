using uBeac.Messaging.RabbitMQ;

namespace uBeac.HttpHub.Producers
{
    public class HubProducer: Producer
    {
        public HubProducer(MessagingClientOptions<HubProducer> messagingClientOptions) : base(messagingClientOptions)
        {
        }
    }
}
