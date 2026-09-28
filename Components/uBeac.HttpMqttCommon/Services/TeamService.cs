using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using uBeac.Models;
using uBeac.Repositories;

namespace uBeac.HttpMqttCommon.Services
{
    public class TeamService : IChangeTrackerStartupService
    {
        IChangeTracker<Guid, Team> _changeTracker;
        private readonly ILogger<TeamService> _logger;
        private readonly IHubService _service;

        public TeamService(IChangeTracker<Guid, Team> changeTracker, ILogger<TeamService> logger, IHubService service)
        {
            _logger = logger;
            _changeTracker = changeTracker;
            _service = service;
            _changeTracker.RegisterForInsert((team) => AddOrUpdateTeam(team, null));
            _changeTracker.RegisterForUpdate((team, x) => AddOrUpdateTeam(team, x));
            _changeTracker.RegisterForDelete((teamId) => DeleteTeam(teamId));
        }

        private void AddOrUpdateTeam(Team team, Dictionary<string, object> updatedFields)
        {
            try
            {
                _service.Teams.AddOrUpdate(team.Id, team.Namespace);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error in AddOrUpdate for team {0} with namespace {1}", team.Id, team.Namespace));
            }

        }
        
        private void DeleteTeam(Guid teamId)
        {
            try
            {
                _service.Teams.Remove(teamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error in deleting team {0} from cache", teamId));
            }
        }

        public void Run()
        {
            _changeTracker.Init();
        }
    }
}
