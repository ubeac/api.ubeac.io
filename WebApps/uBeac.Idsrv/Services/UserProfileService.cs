using System;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Idsrv.Repositories;
using uBeac.Models;

namespace uBeac.Idsrv.Services
{
    public interface IUserProfileService
    {
        Task<bool> UpdateAsync(UserProfile data, CancellationToken cancellationToken = default);
        Task UpdateLastActivityDate(Guid userId, CancellationToken cancellationToken = default);
        Task<UserProfile> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UserProfile> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }

    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepository;

        public UserProfileService(IUserProfileRepository userProfileRepository)
        {
            _userProfileRepository = userProfileRepository;
        }

        public async Task<UserProfile> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _userProfileRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<UserProfile> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _userProfileRepository.GetByEmailAsync(email, cancellationToken);
        }

        public async Task<bool> UpdateAsync(UserProfile data, CancellationToken cancellationToken = default)
        {
            return await _userProfileRepository.UpdateAsync(data, cancellationToken);
        }

        public async Task UpdateLastActivityDate(Guid userId, CancellationToken cancellationToken = default)
        {
            await _userProfileRepository.UpdateLastActivityDate(userId, cancellationToken);
        }
    }
}
