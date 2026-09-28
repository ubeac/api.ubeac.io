using System;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Repositories;
using uBeac.Models;

namespace uBeac.Api.Services
{
    public interface IUserProfileService 
    {
        Task<UserProfile> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserProfile> GetAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _repository;

        public UserProfileService(IUserProfileRepository repository) 
        {
            _repository = repository;
        }

        public async Task<UserProfile> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByEmailAsync(email, cancellationToken);
        }

        public async Task<UserProfile> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

    }
}
