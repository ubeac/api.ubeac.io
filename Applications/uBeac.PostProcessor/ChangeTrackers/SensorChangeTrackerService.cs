using System;
using System.Collections.Generic;
using System.Linq;
using uBeac.Models;
using uBeac.PostProcessor.Models;
using uBeac.PostProcessor.Repositories;
using uBeac.Repositories;

namespace uBeac.PostProcessor.ChangeTrackers
{
    public class SensorChangeTrackerService : IChangeTrackerStartupService
    {
        IChangeTracker<Guid, Sensor> _changeTracker;
        private readonly TeamsModel _teamsModel;
        private readonly IRepository _repository;

        public SensorChangeTrackerService(IChangeTracker<Guid, Sensor> changeTracker, TeamsModel teamsModel, IRepository repository)
        {
            _changeTracker = changeTracker;
            _teamsModel = teamsModel;
            _repository = repository;
            _changeTracker.RegisterForInsert((sensor) => AddSensor(sensor));
            _changeTracker.RegisterForUpdate((sensor, x) => UpdateSensor(sensor, x));
            _changeTracker.RegisterForDelete((sensorId) => DeleteSensor(sensorId));
        }

        private void AddSensor(Sensor sensor)
        {
            _teamsModel.TryAddSensor(sensor.TeamId, sensor.DeviceId, sensor.Id, sensor.Uid, sensor.Persist, sensor.Schema.ToHashSet());
        }

        private void UpdateSensor(Sensor sensor, Dictionary<string, object> updatedFields)
        {
            if (updatedFields == null)
                return;

            if (updatedFields.ContainsKey("Persist") || updatedFields.ContainsKey("Schema"))
                if (_teamsModel.TryUpdateSensor(sensor.Id, sensor.Persist, sensor.Schema.ToHashSet()))
                    _repository.UpdateSensorSchema(sensor.Id, sensor.Persist, sensor.Schema);
        }

        private void DeleteSensor(Guid sensorId)
        {
            _teamsModel.RemoveSensor(sensorId);
        }

        public void Run()
        {
            _changeTracker.Init();
        }
    }
}
