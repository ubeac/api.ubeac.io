/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

namespace uBeac.Messaging.RabbitMQ
{
    public class MessagingClientOptions<TClient> : MessagingClientOptions where TClient : IMessagingClient
    {
        public MessagingClientOptions(MessagingClientSettings messagingClientSettings) : base(messagingClientSettings)
        {
        }
    }

    public class MessagingClientOptions
    {
        public MessagingClientSettings MessagingClientSettings { get; set; }
        public MessagingClientOptions(MessagingClientSettings messagingClientSettings)
        {
            MessagingClientSettings = messagingClientSettings;
        }
    }
}
