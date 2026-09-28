using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uBeac.Api.Facades;
using uBeac.Api.InputModels;
using uBeac.Models;

namespace uBeac.Api.Controllers
{
    [SwaggerTag(TeamConstants.DESCRIPTION)]
    public class TeamController : BaseEntityController<ITeamFacade, Team, TeamInputModelAdd, TeamInputModelUpdate>
    {
        private readonly ITeamFacade _teamFacade;

        public TeamController(ITeamFacade teamFacade) : base(teamFacade)
        {
            _teamFacade = teamFacade;
        }

        [HttpGet]
        [SwaggerOperation(Summary = TeamConstants.GETALL_SUMMARY, Description = TeamConstants.GETALL_DESCRIPTION)]
        public async Task<ResultSet<List<Team>>> GetAll()
        {
            return await _teamFacade.GetAllAsync();
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = TeamConstants.GETBYID_SUMMARY, Description = TeamConstants.GETBYID_DESCRIPTION)]
        public async Task<ResultSet<Team>> GetById(Guid id)
        {
            return await _teamFacade.GetByIdAsync(id);
        }

        [SwaggerOperation(Summary = TeamConstants.ADD_SUMMARY, Description = TeamConstants.ADD_DESCRIPTION)]
        public override async Task<ResultSet<Guid>> Add([FromBody, SwaggerParameter("Team", Required = true)] TeamInputModelAdd team)
        {
            var model = Mapping.Mapper.Map<Team>(team);
            model.Namespace = model.Namespace.ToLower();

            if (model.Namespace.StartsWith("momentaj") || model.Namespace.StartsWith("ubeac"))
                return new ResultSet<Guid>(Guid.Empty, ResponseCodes.BadRequest);

            return await _teamFacade.AddAsync(model);
        }

        [SwaggerOperation(Summary = TeamConstants.UPDATE_SUMMARY, Description = TeamConstants.UPDATE_DESCRIPTION)]
        public override async Task<ResultSet<bool>> Update([FromBody, SwaggerParameter("Team", Required = true)] TeamInputModelUpdate team)
        {
            var model = Mapping.Mapper.Map<Team>(team);
            model.TeamId = model.Id;
            model.Namespace = model.Namespace.ToLower();

            if (model.Namespace.StartsWith("momentaj") || model.Namespace.StartsWith("ubeac"))
                return new ResultSet<bool>(false, ResponseCodes.BadRequest);

            return await _teamFacade.UpdateAsync(model);
        }

        [SwaggerOperation(Summary = TeamConstants.DELETE_SUMMARY, Description = TeamConstants.DELETE_DESCRIPTION)]
        public override async Task<ResultSet<bool>> Remove(Guid id)
        {
            return await base.Remove(id);
        }

        [HttpGet("{teamNamespace}")]
        [SwaggerOperation(Summary = TeamConstants.EXISTS_SUMMARY, Description = TeamConstants.EXISTS_DESCRIPTION)]
        public async Task<ResultSet<bool>> Exists(string teamNamespace)
        {
            return await _teamFacade.ExistsAsync(teamNamespace);
        }

        [HttpPost]
        public async Task<ResultSet<string>> InvokeToken([FromBody] TeamInputModelGetToken model)
        {
            return await _teamFacade.InvokeTokenAsync(model.TeamId, model.AccessLevel);
        }

        [HttpPost]
        public async Task<ResultSet<bool>> RemoveToken([FromBody] TeamInputModelRemoveToken model)
        {
            return await _teamFacade.RemoveTokenAsync(model.TeamId, model.Token);
        }
    }
}
