using System;
using uBeac.Models;
using uBeac.PostProcessor.Models;
using uBeac.Repositories;

namespace uBeac.PostProcessor.ChangeTrackers
{
    public class DeviceChangeTrackerService : IChangeTrackerStartupService
    {
        IChangeTracker<Guid, Device> _changeTracker;
        private readonly TeamsModel _teamsModel;

        public DeviceChangeTrackerService(IChangeTracker<Guid, Device> changeTracker, TeamsModel teamsModel)
        {
            _changeTracker = changeTracker;
            _teamsModel = teamsModel;
            _changeTracker.RegisterForInsert((device) => AddDevice(device));
            _changeTracker.RegisterForDelete((deviceId) => DeleteDevice(deviceId));
        }

        private void AddDevice(Device device)
        {
            _teamsModel.TryAddDevice(device.TeamId, device.Id, device.Uid);
        }

        private void DeleteDevice(Guid deviceId)
        {
            _teamsModel.RemoveDevice(deviceId);
        }

        public void Run()
        {
            _changeTracker.Init();
        }
    }
}
