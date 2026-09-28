using uBeac.Messaging.RabbitMQ;

namespace uBeac.MqttHub.Producers
{
    public class MqttHubProducer: Producer
    {
        public MqttHubProducer(MessagingClientOptions<MqttHubProducer> messagingClientOptions) : base(messagingClientOptions)
        {
        }
    }
}
