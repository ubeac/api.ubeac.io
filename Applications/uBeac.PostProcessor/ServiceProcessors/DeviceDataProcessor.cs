using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.PostProcessor.Models;

namespace uBeac.PostProcessor.ServiceProcessors
{
    public class DeviceDataExtractor : IProcessor
    {
        private readonly TeamsModel _teamsModel;
        private readonly ILogger<DeviceDataExtractor> _logger;
        public DeviceDataExtractor(TeamsModel teamsModel, ILogger<DeviceDataExtractor> logger)
        {
            _teamsModel = teamsModel;
            _logger = logger;
        }
        public async Task ProcessAsync(GatewayData gatewayData, DeviceRawData deviceRawData)
        {
            var deviceUid = deviceRawData.Uid;

            try
            {
                if (!_teamsModel.TryGetDeviceByUid(gatewayData.TeamId, deviceUid, out DeviceModel deviceModel))
                    return;

                var deviceData = new DeviceData(deviceRawData, deviceModel.Id);
                foreach (var sensorRawData in deviceRawData.Sensors)
                {
                    if (_teamsModel.TryGetSensorByUid(gatewayData.TeamId, deviceModel.Uid, sensorRawData.SensorUid, out SensorModel sensorModel))
                    {
                        var sensorData = new SensorData(sensorRawData, sensorModel.Id, gatewayData.GatewayId);
                        deviceData.Sensors.Add(sensorData);
                    }
                }

                gatewayData.Devices.Add(deviceData);

                await Task.FromResult(0);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in extracting device data");
            }


        }
    }
}
