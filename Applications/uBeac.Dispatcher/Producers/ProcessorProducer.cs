namespace uBeac.Messaging.RabbitMQ
{
    public class ProcessorProducer : Producer
    {
        public ProcessorProducer(MessagingClientOptions<ProcessorProducer> messagingClientOptions) : base(messagingClientOptions)
        {
        }   
    }
}
