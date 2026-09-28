/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace uBeac.Messaging.RabbitMQ
{
    public abstract class Consumer : IConsumer
    {
        protected IConnectionFactory ConnectionFactory { get; private set; }
        protected IConnection Connection { get; private set; }
        protected IModel Channel { get; private set; }
        protected MessagingClientSettings MessagingClientSettings { get; private set; }
        protected EventingBasicConsumer EventingBasicConsumer { get; private set; }
        private readonly ILogger<Consumer> _logger;
        private readonly ExceptionProducer _exceptionProducer;

        public Consumer(MessagingClientOptions messagingClientOptions, ILogger<Consumer> logger)
        {
            _logger = logger;

            // preparing exception queue
            var exceptionClientSettings = new MessagingClientSettings()
            {
                BatchSize = messagingClientOptions.MessagingClientSettings.BatchSize,
                ConnectionString = messagingClientOptions.MessagingClientSettings.ConnectionString,
                ConnectionStringName = messagingClientOptions.MessagingClientSettings.ConnectionStringName,
                QueueName = "_" + messagingClientOptions.MessagingClientSettings.QueueName
            };

            var exceptionClientOptions = new MessagingClientOptions<ExceptionProducer>(exceptionClientSettings);
            _exceptionProducer = new ExceptionProducer(exceptionClientOptions);

            // preparing consumer settings
            MessagingClientSettings = messagingClientOptions.MessagingClientSettings;

            ConnectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(MessagingClientSettings.ConnectionString),
            };

            Connection = ConnectionFactory.CreateConnection();
            Channel = Connection.CreateModel();
            Channel.QueueDeclare(queue: MessagingClientSettings.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            if (MessagingClientSettings.BatchSize.HasValue)
            {
                Channel.BasicQos(0, MessagingClientSettings.BatchSize.Value, false);
            }
        }

        public abstract Task Handler(object sender, DeliverEventArgs deliverEventArgs);

        public void Init()
        {
            EventingBasicConsumer = new EventingBasicConsumer(Channel);

            EventingBasicConsumer.Received += async (model, ea) =>
             {
                 var eventArgs = new DeliverEventArgs(ea.Body);

                 try
                 {
                     var startDate = DateTime.UtcNow;

                     await Handler(this, eventArgs);

                     var endDate = DateTime.UtcNow;
                     // logging the successful sent 
                     using (_logger.BeginScope("{@Object}{ReceivedDate}{SentDate}{Duration}", eventArgs.Object, startDate, endDate, (endDate - startDate).TotalMilliseconds))
                         _logger.LogInformation("OK!");

                 }
                 catch (Exception ex)
                 {
                     _logger.LogError(ex, "NOK!");
                     // send message to exception queue
                     await _exceptionProducer.SendBytes(eventArgs.Bytes);
                 }

                 Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

             };
            Channel.BasicConsume(queue: MessagingClientSettings.QueueName, autoAck: false, consumer: EventingBasicConsumer);
        }

        public void Dispose()
        {
            Channel.Close();
            Connection.Close();

            Channel.Dispose();
            Connection.Dispose();
        }

    }
}
