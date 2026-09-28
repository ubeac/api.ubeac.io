using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.PostProcessor.Models;
using uBeac.PostProcessor.Repositories;

namespace uBeac.PostProcessor.ServiceProcessors
{
    public class ExistingDeviceProcessor : IProcessor
    {
        private readonly TeamsModel _teamsModel;
        private readonly IRepository _repository;
        private readonly ILogger<ExistingDeviceProcessor> _logger;
        public ExistingDeviceProcessor(TeamsModel teamsModel, IRepository repository, ILogger<ExistingDeviceProcessor> logger)
        {
            _repository = repository;
            _teamsModel = teamsModel;
            _logger = logger;
        }
        public async Task ProcessAsync(GatewayData gatewayData, DeviceRawData deviceRawData)
        {
            try
            {
                var deviceUid = deviceRawData.Uid;

                if (!_teamsModel.TryGetDeviceByUid(gatewayData.TeamId, deviceUid, out DeviceModel deviceModel))
                    return;

                var newSensors = new List<Sensor>();
                var updateSchemaTasks = new List<Task>();
                foreach (var sensorRawData in deviceRawData.Sensors)
                {
                    if (!_teamsModel.TryGetSensorByUid(gatewayData.TeamId, deviceModel.Uid, sensorRawData.SensorUid, out SensorModel sensorModel))
                    {
                        var newSensor = new Sensor(sensorRawData, gatewayData.TeamId, deviceModel.Id);
                        
                        if(_teamsModel.TryAddSensor(gatewayData.TeamId, deviceModel.Id, newSensor.Id, newSensor.Uid, newSensor.Persist, newSensor.Schema.ToHashSet()))
                            newSensors.Add(newSensor);
                    }
                    else
                    {
                        var updatable = false;
                        foreach (var key in sensorRawData.Data.Keys)
                        {
                            if (!sensorModel.Schema.Contains(key))
                            {
                                sensorModel.Schema.Add(key);
                                updatable = true;
                            }
                        }
                        if (updatable)
                        {
                            _teamsModel.TryUpdateSensor(sensorModel.Id, sensorModel.Persist, sensorModel.Schema);
                            updateSchemaTasks.Add(_repository.UpdateSensorSchema(sensorModel.Id, sensorModel.Persist, sensorModel.Schema.ToList()));
                        }

                    }
                }

                if (newSensors.Count > 0)
                    await _repository.InsertManySensorAsync(newSensors);
                if (updateSchemaTasks.Count > 0)
                    await Task.WhenAll(updateSchemaTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in processing Existing Device");
            }
            
        }
    }
}
