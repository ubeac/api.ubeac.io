using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uBeac.Messaging;
using uBeac.Messaging.RabbitMQ;
using uBeac.Models;
using uBeac.WebSocket;

namespace uBeac.SocketApi
{
    public class SocketConsumer : Consumer
    {
        private readonly ISocket _socket;

        public SocketConsumer(MessagingClientOptions<SocketConsumer> messagingClientOptions, ILogger<SocketConsumer> logger, ISocket socket) : base(messagingClientOptions, logger)
        {
            _socket = socket;
            Init();
        }

        public override async Task Handler(object sender, DeliverEventArgs deliverEventArgs)
        {
            // deserialize Gateway data
            var gatewayData = deliverEventArgs.Get<GatewayData>();

            // check if the data is expired or not
            if ((DateTime.UtcNow - gatewayData.DateTime).TotalSeconds > 60)
                return;

            var allTasks = new List<Task>();

            var teamId = gatewayData.TeamId.ToString();
            var gatewayId = gatewayData.GatewayId.ToString();

            // Gateway Section, sending live Gateway
            allTasks.PublishGateway(_socket, gatewayData);

            // GatewayData Section, sending data to Gateway info detail page
            allTasks.PublishGatewayData(_socket, gatewayData);

            await Task.WhenAll(allTasks.ToArray());
            allTasks.Clear();

            // Device RAW data Section
            allTasks.PublishDeviceRawData(_socket, gatewayData);

            await Task.WhenAll(allTasks.ToArray());
            allTasks.Clear();

            foreach (var deviceData in gatewayData.Devices)
            {
                // Device Data Section
                var deviceId = deviceData.Id.ToString();
                allTasks.PublishDeviceData(_socket, teamId, gatewayId, deviceData);

                // Sensor Data Section
                foreach (var sensorData in deviceData.Sensors)
                {
                    allTasks.PublishSensorData(_socket, teamId, gatewayId, deviceId, sensorData);
                }

                await Task.WhenAll(allTasks);
                allTasks.Clear();
            }
        }
    }
}
