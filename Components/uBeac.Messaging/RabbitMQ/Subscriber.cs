/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace uBeac.Messaging.RabbitMQ
{
    public abstract class Subscriber : ISubscriber
    {

        protected IConnectionFactory ConnectionFactory { get; private set; }
        protected IConnection Connection { get; private set; }
        protected IModel Channel { get; private set; }
        protected MessagingClientSettings MessagingClientSettings { get; private set; }

        public Subscriber(MessagingClientOptions messagingClientOptions)
        {
            MessagingClientSettings = messagingClientOptions.MessagingClientSettings;

            ConnectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(MessagingClientSettings.ConnectionString),
            };

            Connection = ConnectionFactory.CreateConnection();
            Channel = Connection.CreateModel();

            Channel.ExchangeDeclare(exchange: MessagingClientSettings.ExchangeName, type: "fanout");

            var queueName = Channel.QueueDeclare().QueueName;
            Channel.QueueBind(queue: queueName, exchange: MessagingClientSettings.ExchangeName, routingKey: "");


            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (model, ea) =>
            {
                var eventArgs = new DeliverEventArgs(ea.Body);
                Handler(this, eventArgs);
            };

            Channel.BasicConsume(queue: MessagingClientSettings.QueueName, autoAck: true, consumer: consumer);

        }

        public abstract void Handler(object sender, DeliverEventArgs deliverEventArgs);

        public void Dispose()
        {
            Channel.Close();
            Connection.Close();

            Channel.Dispose();
            Connection.Dispose();
        }

    }
}
