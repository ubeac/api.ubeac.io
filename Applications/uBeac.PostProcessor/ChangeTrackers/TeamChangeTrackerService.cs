using System;
using uBeac.Models;
using uBeac.PostProcessor.Models;
using uBeac.Repositories;

namespace uBeac.PostProcessor.ChangeTrackers
{
    public class TeamChangeTrackerService : IChangeTrackerStartupService
    {
        IChangeTracker<Guid, Team> _changeTracker;
        private readonly TeamsModel _teamsModel;

        public TeamChangeTrackerService(IChangeTracker<Guid, Team> changeTracker, TeamsModel teamsModel)
        {
            _changeTracker = changeTracker;
            _teamsModel = teamsModel;
            _changeTracker.RegisterForInsert((team) => AddTeam(team));
            _changeTracker.RegisterForDelete((teamId) => DeleteTeam(teamId));
        }

        private void AddTeam(Team team)
        {
            _teamsModel.Add(team.Id);
        }

        private void DeleteTeam(Guid teamId)
        {
            _teamsModel.Remove(teamId);
        }

        public void Run()
        {
            _changeTracker.Init();
        }
    }
}
