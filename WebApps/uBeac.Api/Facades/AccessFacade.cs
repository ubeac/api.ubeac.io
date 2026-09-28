using System;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Services;
using uBeac.Models;
using uBeac.Security;
using IAccessService = uBeac.Api.Services.IAccessService;

namespace uBeac.Api.Facades
{
    public interface IAccessFacade
    {
        Task<ResultSet<Guid>> AddAsync(Access access, CancellationToken cancellationToken = default);
        Task<ResultSet<bool>> UpdateAsync(Access access, CancellationToken cancellationToken = default);
        Task<ResultSet<bool>> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public class AccessFacade : IAccessFacade
    {
        private readonly ISecurityContext _securityContext;
        private readonly IAccessService _accessService;
        private readonly IEmailService _emailService;
        private readonly IUserProfileService _userProfileService;
        private readonly ITeamService _teamService;

        public AccessFacade(ISecurityContext securityContext,
                            IAccessService accessService,
                            IEmailService emailService,
                            IUserProfileService userProfileService,
                            ITeamService teamService)
        {
            _securityContext = securityContext;
            _accessService = accessService;
            _emailService = emailService;
            _userProfileService = userProfileService;
            _teamService = teamService;
        }

        public async Task<ResultSet<Guid>> AddAsync(Access access, CancellationToken cancellationToken = default)
        {
            if (access.TeamId == null || access.TeamId == Guid.Empty || !_securityContext.HasAccess<Team>(AccessLevels.Admin, access.TeamId))
                return new ResultSet<Guid>(Guid.Empty, ResponseCodes.UnAuthorized);

            bool exist = await _accessService.ExistAsync(access.UserId, access.TeamId, cancellationToken);
            if (exist)
                return new ResultSet<Guid>(Guid.Empty, ResponseCodes.NotAcceptable);

            access.CreateBy = _securityContext.Identity.UserId;

            var result = await _accessService.AddAsync(access, cancellationToken);

            if (result == Guid.Empty)
                return new ResultSet<Guid>(result, ResponseCodes.BadRequest);

            var team = await _teamService.GetByIdAsync(access.TeamId);
            var userProfile = await _userProfileService.GetAsync(access.UserId, cancellationToken);
            await _emailService.SendUserAdded(userProfile.Email, team.Name);

            return new ResultSet<Guid>(result);

        }

        public async Task<ResultSet<bool>> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var access = await _accessService.GetByIdAsync(id, cancellationToken);
            // access does not exist
            if (access is null)
                return new ResultSet<bool>(false, ResponseCodes.NotFound);

            // current user has not access to the access's team
            if (!_securityContext.HasAccess<Team>(AccessLevels.Admin, access.TeamId))
                return new ResultSet<bool>(false, ResponseCodes.UnAuthorized);

            var result = await _accessService.DeleteAsync(id, cancellationToken);
            return new ResultSet<bool>(result);
        }

        public async Task<ResultSet<bool>> UpdateAsync(Access access, CancellationToken cancellationToken = default)
        {
            // current user has not access to the access's team
            if (access.TeamId == null || access.TeamId == Guid.Empty || !_securityContext.HasAccess<Team>(AccessLevels.Admin, access.TeamId))
                return new ResultSet<bool>(false, ResponseCodes.UnAuthorized);

            // there is no need to keep update by and we always set createby
            access.CreateBy = _securityContext.Identity.UserId;

            var result = await _accessService.UpdateAsync(access, cancellationToken);
            return new ResultSet<bool>(result);
        }

    }

}
