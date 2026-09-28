using System.Linq;
using System.Threading.Tasks;
using uBeac.Messaging.RabbitMQ;
using uBeac.Models;
using uBeac.PostProcessor.Models;

namespace uBeac.PostProcessor.Producers
{
    public class SensorProducer : Producer
    {
        TeamsModel _teamsModel;
        public SensorProducer(MessagingClientOptions<SensorProducer> messagingClientOptions, TeamsModel teamsModel) : base(messagingClientOptions)
        {
            _teamsModel = teamsModel;
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

            foreach (var deviceData in data.Devices)
            {
                foreach (var sensorData in deviceData.Sensors.ToList())
                {
                    if(_teamsModel.TryGetSensorById(data.TeamId, deviceData.Uid, sensorData.SensorId, out SensorModel sensorModel))
                    {
                        if (!sensorModel.Persist)
                            deviceData.Sensors.Remove(sensorData);
                    }
                }
            }

            await Send<GatewayData>(data);
        }
    }
}
