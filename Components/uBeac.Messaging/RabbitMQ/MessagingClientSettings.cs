/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

namespace uBeac.Messaging.RabbitMQ
{
    public class MessagingClientSettings
    {
        public string ConnectionStringName { get; set; }
        public string QueueName { get; set; }
        public string ConnectionString { get; set; }
        public string ExchangeName { get; set; }
        public ushort? BatchSize { get; set; }
    }
}
