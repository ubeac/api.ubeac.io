/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace uBeac.Messaging.RabbitMQ
{
    public abstract class Producer : IProducer
    {
        protected readonly IConnectionFactory ConnectionFactory;
        protected readonly IConnection Connection;
        protected readonly IModel Channel;
        protected readonly MessagingClientSettings MessagingClientSettings;

        public Producer(MessagingClientOptions messagingClientOptions)
        {
            MessagingClientSettings = messagingClientOptions.MessagingClientSettings;

            ConnectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(MessagingClientSettings.ConnectionString),
            };

            Connection = ConnectionFactory.CreateConnection();

            Channel = Connection.CreateModel();
            Channel.QueueDeclare(queue: MessagingClientSettings.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        }

        public void Dispose()
        {
            Channel.Close();
            Connection.Close();

            Channel.Dispose();
            Connection.Dispose();
        }
        
        public virtual async Task Send<T>(T data)
        {
            var bytes = Serialization.MessageSerilizer.Serialize(data);
            await SendBytes(bytes);
        }

        public async Task SendBytes(byte[] bytes)
        {
            Channel.BasicPublish(exchange: "", routingKey: MessagingClientSettings.QueueName, basicProperties: null, body: bytes);
            await Task.FromResult(0);
        }
    }
}
