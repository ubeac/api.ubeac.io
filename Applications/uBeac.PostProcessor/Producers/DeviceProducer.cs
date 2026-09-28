using System.Threading.Tasks;
using uBeac.Messaging.RabbitMQ;
using uBeac.Models;

namespace uBeac.PostProcessor.Producers
{
    public class DeviceProducer : Producer
    {
        public DeviceProducer(MessagingClientOptions<DeviceProducer> messagingClientOptions) : base(messagingClientOptions)
        {
        }
        public async Task Send(GatewayData gatewayData)
        {
            if (gatewayData.Devices.Count == 0)
                return;

            var data = new GatewayData
            {
                Id = gatewayData.Id,
                TeamId = gatewayData.TeamId,
                FloorId = gatewayData.FloorId,
                GatewayId = gatewayData.GatewayId,
                DateTime = gatewayData.DateTime,
                Devices = gatewayData.Devices               
            };
            await Send<GatewayData>(data);
        }
    }
}
