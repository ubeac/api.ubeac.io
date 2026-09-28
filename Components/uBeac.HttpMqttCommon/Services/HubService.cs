using Microsoft.Extensions.Logging;
using uBeac.HttpMqttCommon.Models;
using uBeac.HttpMqttCommon.Repositories;

namespace uBeac.HttpMqttCommon.Services
{
    public interface IHubService
    {
        TeamsModel Teams { get; set; }
    }

    public class HubService : IHubService
    {
        public TeamsModel Teams { get; set; }
        private readonly IHubRepository _hubRepository;
        private readonly ILogger<HubService> _logger;
        public HubService(IHubRepository hubRepository, ILogger<HubService> logger)
        {
            Teams = new TeamsModel();
            _logger = logger;
            _hubRepository = hubRepository;
            Init();
        }

        // todo: implement cancellationtoken
        private void Init()
        {
            var gatewayList = _hubRepository.GetAllGateways().Result;
            var teamList = _hubRepository.GetAllTeams().Result;

            foreach (var team in teamList)
            {
                Teams.AddOrUpdate(team.Id, team.Namespace);
            }

            foreach (var gateway in gatewayList)
            {
                if (Teams.TryGetById(gateway.TeamId, out TeamModel teamModel))
                    teamModel.Gateways.AddOrUpdate(gateway.Id, gateway);
                else
                    _logger.LogWarning(string.Format("TeamId {0} doesn't exist in cache for GatewayId {1}.", gateway.TeamId.ToString(), gateway.Id.ToString()));
            }
        }
    }

}
