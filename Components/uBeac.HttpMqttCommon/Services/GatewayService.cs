using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using uBeac.Models;
using uBeac.Repositories;

namespace uBeac.HttpMqttCommon.Services
{
    public class GatewayService : IChangeTrackerStartupService
    {
        IChangeTracker<Guid, Gateway> _changeTracker;
        private readonly ILogger<GatewayService> _logger;
        private readonly IHubService _service;

        public GatewayService(IChangeTracker<Guid, Gateway> changeTracker, ILogger<GatewayService> logger, IHubService service)
        {
            _logger = logger;
            _service = service;
            _changeTracker = changeTracker;
            _changeTracker.RegisterForInsert((gateway) => AddOrUpdateGateway(gateway, null));
            _changeTracker.RegisterForUpdate((gateway, x) => AddOrUpdateGateway(gateway, x));
            _changeTracker.RegisterForDelete((gatewayId) => DeleteGateway(gatewayId));
        }

        private void AddOrUpdateGateway(Gateway gateway, Dictionary<string, object> updatedFields)
        {
            if (_service.Teams.TryGetById(gateway.TeamId, out Models.TeamModel teamModel))
            {
                teamModel.Gateways.AddOrUpdate(gateway.Id, gateway);
            }
            else
            {
                _logger.LogError(string.Format("Critical error: TeamId {0} does not exists for GatewayId {1}. Review completely for Insert/Update in MongoDB ChangeStream!", gateway.TeamId.ToString(), gateway.Id.ToString()));
            }
        }

        private void DeleteGateway(Guid gatewayId)
        {
            if (_service.Teams.Gateways.TryGetValue(gatewayId, out Gateway tempGateway))
            {
                if (_service.Teams.TryGetById(tempGateway.TeamId, out Models.TeamModel tempTeamModel))
                {
                    tempTeamModel.Gateways.Remove(gatewayId);
                }
            }
            else
            {
                _logger.LogError(string.Format("Critical error: GatewayId {0} does not exists. Review completely for Deletion in MongoDB ChangeStream!", gatewayId.ToString()));
            }
        }

        public void Run()
        {
            _changeTracker.Init();
        }
    }
}
