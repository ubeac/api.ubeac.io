/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

namespace uBeac.Messaging.RabbitMQ
{
    // todo: find a better way to handle exceptions in queue
    public class ExceptionProducer : Producer
    {
        public ExceptionProducer(MessagingClientOptions<ExceptionProducer> messagingClientOptions) : base(messagingClientOptions)
        {
        }
    }
}
