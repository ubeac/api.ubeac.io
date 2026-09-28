using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using uBeac.Models;
using uBeac.Notifications;
using uBeac.Notifications.Models;
using uBeac.Repositories;
using uBeac.WebSocket;

namespace uBeac.SocketApi.Services
{
    // todo: async implementation
    public class DeviceRequestService : IChangeTrackerStartupService
    {
        IChangeTracker<Guid, DeviceSummary> _changeTracker;
        private readonly ISocket _socket;
        private readonly ILogger<DeviceRequestService> _logger;

        const string DELIMITER = "/";
        const string CHANGELOG_PREFIX = "ChangeLog";
        private string _typeName = typeof(DeviceDataRec).Name;

        public DeviceRequestService(IChangeTracker<Guid, DeviceSummary> changeTracker, ISocket socket, ILogger<DeviceRequestService> logger)
        {
            _socket = socket;
            _logger = logger;
            _changeTracker = changeTracker;
            _changeTracker.RegisterForInsert(deviceSummary => SendDeviceRequestCount(deviceSummary, null));
            _changeTracker.RegisterForUpdate((deviceSummary, x) => SendDeviceRequestCount(deviceSummary, x));
        }

        // we just send a new DeviceDataRec which contains 1 value (not real request count for the device)
        private void SendDeviceRequestCount(DeviceSummary deviceSummary, Dictionary<string, object> updatedFields)
        {
            try
            {
                ChangeLog changeLog = new ChangeLog()
                {
                    TeamId = deviceSummary.TeamId,
                    Id = deviceSummary.DeviceId,
                    Value = new DeviceDataRec(),
                    Action = ActionTypes.Updated,
                    Type = _typeName
                };

                string changeLogGroupId = CHANGELOG_PREFIX + DELIMITER + deviceSummary.TeamId.ToString();

                _socket.SendToGroupAsync(changeLogGroupId, changeLog).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error in sending DeviceRequestCount for deviceSummary {0}", deviceSummary.Id));
            }

        }

        public void Run()
        {
            _changeTracker.Init();
        }
    }
}
