using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;

namespace uBeac.Security
{
    public interface IAccessService
    {
        Task<List<Access>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    }

    public class AccessService : IAccessService
    {
        private readonly IAccessRepository _accessRepository;

        public AccessService(IAccessRepository accessRepository)
        {
            _accessRepository = accessRepository;
        }
        
        public async Task<List<Access>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _accessRepository.GetByUserIdAsync(userId, cancellationToken);
        }
    }
}
