using uBeac.Models;
using uBeac.Api.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using uBeac.Security;

namespace uBeac.Api.Facades
{
    public interface ITeamFacade : IBaseEntityFacade<Team>
    {
        Task<ResultSet<List<Team>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ResultSet<bool>> ExistsAsync(string teamNamespace, CancellationToken cancellationToken = default);
        Task<ResultSet<string>> InvokeTokenAsync(Guid teamId, AccessLevels accessLevel, CancellationToken cancellationToken = default);
        Task<ResultSet<bool>> RemoveTokenAsync(Guid teamId, string token, CancellationToken cancellationToken = default);
    }

    public class TeamFacade : BaseEntityFacade<Team>, ITeamFacade
    {
        private readonly ITeamService _teamService;

        public TeamFacade(ITeamService teamService, ISecurityContext securityContext) : base(teamService, securityContext)
        {
            _teamService = teamService;
        }

        public async Task<ResultSet<List<Team>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var teamIds = SecurityContext.GetAccessible<Team>(AccessLevels.View);
            var teams = await _teamService.GetByIdsAsync(teamIds, cancellationToken);
            teams.ForEach(team => team.Tokens.Clear());

            return new ResultSet<List<Team>>(teams);
        }

        public override async Task<ResultSet<Guid>> AddAsync(Team model, CancellationToken cancellationToken = default)
        {
            if (SecurityContext.Identity.UserId == null || SecurityContext.Identity.UserId == Guid.Empty)
                return new ResultSet<Guid>(Guid.Empty, ResponseCodes.UnAuthorized);

            model.CreateBy = SecurityContext.Identity.UserId;
            model.UpdateBy = SecurityContext.Identity.UserId;

            var result = await _teamService.AddAsync(model, cancellationToken);

            if (result != Guid.Empty)            
                return new ResultSet<Guid>(result);            

            return new ResultSet<Guid>(result, ResponseCodes.BadRequest);
        }

        public override async Task<ResultSet<Team>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var teamResult = await base.GetByIdAsync(id, cancellationToken);
            var team = teamResult.Data;

            // removing sensitive data for view users
            if (team != null && !SecurityContext.HasAccess<Team>(AccessLevels.Admin, team.Id))
            {
                team.Tokens.Clear();

                team.Gateways.ForEach(x =>
                {
                    x.Security = null;
                    x.Url = string.Empty;
                });
            }

            return teamResult;
        }

        public override async Task<ResultSet<bool>> UpdateAsync(Team team, CancellationToken cancellationToken = default)
        {
            team.TeamId = team.Id;
            return await base.UpdateAsync(team, cancellationToken);
        }

        public async Task<ResultSet<bool>> ExistsAsync(string teamNamespace, CancellationToken cancellationToken = default)
        {
            return new ResultSet<bool>(await _teamService.ExistsAsync(teamNamespace, cancellationToken));
        }

        public async Task<ResultSet<string>> InvokeTokenAsync(Guid teamId, AccessLevels accessLevel, CancellationToken cancellationToken = default)
        {
            if (!SecurityContext.HasAccess<Team>(AccessLevels.Admin, teamId))
                return new ResultSet<string>(ResponseCodes.UnAuthorized);

            return await _teamService.InvokeTokenAsync(teamId, accessLevel, cancellationToken);
        }

        public async Task<ResultSet<bool>> RemoveTokenAsync(Guid teamId, string token, CancellationToken cancellationToken = default)
        {
            if (!SecurityContext.HasAccess<Team>(AccessLevels.Admin, teamId))
                return new ResultSet<bool>(ResponseCodes.UnAuthorized);

            return await _teamService.RemoveTokenAsync(teamId, token, cancellationToken);
        }
    }

}
