using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Repositories;
using uBeac.Models;

namespace uBeac.Api.Services
{
    public interface IAccessService
    {
        Task<List<Access>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<long> DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<Guid> AddAsync(Access access, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Access access, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Access>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Access> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistAsync(Guid userId, Guid teamId, CancellationToken cancellationToken = default);
    }

    public class AccessService : IAccessService
    {
        private readonly IAccessRepository _accessRepository;
        private readonly IUserProfileRepository _userProfileRepository;

        public AccessService(IAccessRepository accessRepository, IUserProfileRepository userProfileRepository)
        {
            _accessRepository = accessRepository;
            _userProfileRepository = userProfileRepository;
        }

        public async Task<Guid> AddAsync(Access access, CancellationToken cancellationToken = default)
        {
            // check if the user exists or not
            var userProfile = await _userProfileRepository.GetByIdAsync(access.UserId);
            if (userProfile is null)
                return Guid.Empty;
            
            access.CreateDate = DateTime.UtcNow;
            access.Id = Guid.NewGuid();

            return await _accessRepository.InsertAsync(access, cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var access = await _accessRepository.GetByIdAsync(id, cancellationToken);
            // access does not exist
            if (access is null)
                return false;

            if (access.Level == AccessLevels.Admin)
            {
                // if the team has just one admin, it should prevent that to be deleted
                var accessList = await _accessRepository.GetByTeamIdAsync(access.TeamId, cancellationToken);
                if (accessList.Where(x => x.Level == AccessLevels.Admin).Count() == 1)
                    return false;
            }
            return await _accessRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<long> DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            return await _accessRepository.DeleteByTeamIdAsync(teamId, cancellationToken);
        }

        public async Task<List<Access>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _accessRepository.GetByUserIdAsync(userId, cancellationToken);
        }

        public async Task<Access> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _accessRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<List<Access>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            return await _accessRepository.GetByTeamIdAsync(teamId, cancellationToken);
        }

        public async Task<bool> UpdateAsync(Access access, CancellationToken cancellationToken = default)
        {
            var oldAccess = await _accessRepository.GetByIdAsync(access.Id);
            // access does not exist
            if (oldAccess is null)
                return false;

            // conflict with team id for the requested access
            if (oldAccess.TeamId != access.TeamId)
                return false;

            // check if the user exists or not
            var userProfile = await _userProfileRepository.GetByIdAsync(access.UserId);
            if (userProfile != null)
                return false;

            // there is no need to keep update date and we always set createdate
            access.CreateDate = DateTime.UtcNow;

            return await _accessRepository.UpdateAsync(access, cancellationToken);
        }

        public async Task<bool> ExistAsync(Guid userId, Guid teamId, CancellationToken cancellationToken = default)
        {
            return await _accessRepository.ExistAsync(userId, teamId, cancellationToken);
        }
    }
}
