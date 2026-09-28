using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.PostProcessor.Models;
using uBeac.PostProcessor.Repositories;

namespace uBeac.PostProcessor.ServiceProcessors
{
    public class NewValidDeviceProcessor : IProcessor
    {
        private readonly TeamsModel _teamsModel;
        private readonly IRepository _repository;
        private readonly ILogger<NewValidDeviceProcessor> _logger;
        public NewValidDeviceProcessor(TeamsModel teamsModel, IRepository repository, ILogger<NewValidDeviceProcessor> logger)
        {
            _teamsModel = teamsModel;
            _repository = repository;
            _logger = logger;
        }
        public async Task ProcessAsync(GatewayData gatewayData, DeviceRawData deviceRawData)
        {
            // Only valid devices are acceptable
            if (!deviceRawData.IsValid)
                return;

            var deviceUid = deviceRawData.Uid;

            try
            {
                // Make sure that is a new device
                if (_teamsModel.TryGetDeviceByUid(gatewayData.TeamId, deviceUid, out DeviceModel deviceModel))
                    return;

                var newDevice = new Device(deviceRawData, gatewayData.TeamId);

                // Add new device to cache
                if (_teamsModel.TryAddDevice(gatewayData.TeamId, newDevice.Id, deviceUid))
                {
                    // Insert new device to DB
                    await _repository.InsertManyDeviceAsync(new List<Device> { newDevice });

                    var newSensors = new List<Sensor>();
                    foreach (var sensorRawData in deviceRawData.Sensors)
                    {
                        var newSensor = new Sensor(sensorRawData, gatewayData.TeamId, newDevice.Id);

                        // Add sensor to cache
                        if (_teamsModel.TryAddSensor(gatewayData.TeamId, newDevice.Id, newSensor.Id, newSensor.Uid, newSensor.Persist, newSensor.Schema.ToHashSet()))
                            newSensors.Add(newSensor);
                    }

                    // Insert sensors to DB
                    if (newSensors.Count > 0)
                        await _repository.InsertManySensorAsync(newSensors);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in processing new devices and new sensors");
            }
        }
    }

}
