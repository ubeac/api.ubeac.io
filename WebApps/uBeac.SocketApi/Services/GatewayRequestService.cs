using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using uBeac.Models;
using uBeac.Notifications;
using uBeac.Notifications.Models;
using uBeac.Repositories;
using uBeac.SocketApi.Repositories;
using uBeac.WebSocket;

namespace uBeac.SocketApi.Services
{
    public class GatewayRequestService : BaseEntityTrackerService<Gateway>
    {

        private readonly ILogger<GatewayRequestService> _logger;
        private readonly ISocket _socket;
        private string _typeName = typeof(GatewayDataRec).Name;
        public GatewayRequestService(IChangeTracker<Guid, Gateway> changeTracker, IRepository<Gateway> repository, ISocket socket, ILogger<GatewayRequestService> logger) : base(changeTracker, repository, socket, logger)
        {
            _logger = logger;
            _socket = socket;
        }

        protected override void SendUpdateNotification(Gateway entity, Dictionary<string, object> updatedFields)
        {
            if (updatedFields is null || !updatedFields.ContainsKey("RequestCount"))
            {
                base.SendUpdateNotification(entity, updatedFields);
                return;
            }

            try
            {
                var changeLog = new ChangeLog()
                {
                    TeamId = entity.TeamId,
                    Id = entity.Id,
                    Value = new GatewayDataRec(),
                    Action = ActionTypes.Updated,
                    Type = _typeName
                };
                _socket.SendToGroupAsync(GetGroupId(entity.TeamId), changeLog).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error in sending request count notification for gateway {0} ", entity.Id.ToString()));
            }
        }
    }
}
