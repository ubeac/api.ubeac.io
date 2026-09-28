/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using RabbitMQ.Client;
using System;

namespace uBeac.Messaging.RabbitMQ
{
    public class Publisher : IPublisher
    {
        protected readonly IConnectionFactory ConnectionFactory;
        protected readonly IConnection Connection;
        protected readonly IModel Channel;
        protected readonly MessagingClientSettings MessagingClientSettings;

        public Publisher(MessagingClientOptions messagingClientOptions)
        {
            MessagingClientSettings = messagingClientOptions.MessagingClientSettings;

            ConnectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(MessagingClientSettings.ConnectionString),
            };

            Connection = ConnectionFactory.CreateConnection();

            Channel = Connection.CreateModel();

            Channel.ExchangeDeclare(exchange: MessagingClientSettings.ExchangeName, type: "fanout");
            
        }

        public void Dispose()
        {
            Channel.Close();
            Connection.Close();

            Channel.Dispose();
            Connection.Dispose();
        }

        public void Publish(byte[] bytes)
        {
            Channel.BasicPublish(exchange: MessagingClientSettings.ExchangeName, routingKey: "", basicProperties: null, body: bytes);
        }
    }
}