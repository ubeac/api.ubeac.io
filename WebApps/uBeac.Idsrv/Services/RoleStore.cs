using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Idsrv.Models;

namespace uBeac.Idsrv.Services
{
    public class RoleStore:
       IQueryableRoleStore<Role>
        //,       IRoleClaimStore<Role>
    {

        private readonly List<Role> _roles;
        public RoleStore()
        {
            _roles = new List<Role>
            {
                new Role(Constants.ROLE_ADMIN),
                new Role(Constants.ROLE_NORMAL_USERS),
                new Role(Constants.ROLE_REGISTERED_EXTERNAL),
                new Role(Constants.ROLE_REGISTERED_INTERNAL)
            };
        }

        public IQueryable<Role> Roles => _roles.AsQueryable();
              
        public Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("RoleService_CreateAsync");
        }

        public Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("RoleService_DeleteAsync");
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var role = _roles.Where(x => x.Name == roleId).SingleOrDefault();
            return Task.FromResult(role);
        }

        public Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var role = _roles.Where(x => x.NormalizedName == normalizedRoleName).SingleOrDefault();
            return Task.FromResult(role);
        }
      

        public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }
        

        public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }



        //public Task AddClaimAsync(Role role, Claim claim, CancellationToken cancellationToken = default)
        //{
        //    throw new NotImplementedException();
        //}
        //public Task<IList<Claim>> GetClaimsAsync(Role role, CancellationToken cancellationToken = default)
        //{
        //    IList<Claim> claims = new List<Claim>();
        //    return Task.FromResult(claims);
        //}
        //public Task RemoveClaimAsync(Role role, Claim claim, CancellationToken cancellationToken = default)
        //{
        //    throw new NotImplementedException();
        //}

    }
}
