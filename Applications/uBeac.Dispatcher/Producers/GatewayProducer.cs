//using System;
//using System.Collections.Generic;
//using uBeac.Messaging.RabbitMQ;

//namespace uBeac.Dispatcher.Producers
//{
//    public class GatewayProducer : Producer
//    {
//        public GatewayProducer(MessagingClientOptions<GatewayProducer> messagingClientOptions) : base(messagingClientOptions)
//        {
//        }
//        public void Send(string gatewayId, DateTime dateTime)
//        {
//            Send(new KeyValuePair<string, DateTime>(gatewayId, dateTime));
//        }
//    }
//}
