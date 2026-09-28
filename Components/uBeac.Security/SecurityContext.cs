using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using uBeac.Models;

namespace uBeac.Security
{
    public interface ISecurityContext
    {
        IUserIdentity Identity { get; }
        bool HasAccess<TEntity>(AccessLevels accessLevel, Guid entityId);
        List<Guid> GetAccessible<TEntity>(AccessLevels accessLevel);
    }
    public class SecurityContext : ISecurityContext
    {
        private readonly List<Access> _accesses;
        public SecurityContext(IHttpContextAccessor httpContextAccessor, IAccessService accessService)
        {
            // setting default empty access list
            _accesses = new List<Access>();

            // preparing curreny context's identity
            Identity = new UserIdentity(httpContextAccessor);

            if (Identity.Authenticated)
            {
                // fetching all accesses for current user
                _accesses = accessService.GetAllAsync(Identity.UserId).Result;

                SetCustomTokenAuthentication(_accesses);

                OverridenAccessLevels(_accesses);
            }
        }

        public IUserIdentity Identity { get; }

        public bool HasAccess<TEntity>(AccessLevels accessLevel, Guid entityId)
        {
            // if the logged in user is system administrator (member of uBeac or Momentaj)
            if (Identity.Roles != null && Identity.Roles.Contains("ADMINS"))
                return true;

            return _accesses.Where(x => x.Level == accessLevel && x.TeamId == entityId).Count() > 0;
        }

        public List<Guid> GetAccessible<TEntity>(AccessLevels accessLevel)
        {
            return _accesses.Where(x => x.Level == accessLevel).Select(x => x.TeamId).Distinct().ToList();
        }

        private void OverridenAccessLevels(List<Access> accesses)
        {
            // if user has admin permission to an entity, we should add view permission as well on the fly
            var adminAcesses = accesses.Where(x => x.Level == AccessLevels.Admin).ToList();

            foreach (var access in adminAcesses)
            {
                accesses.Add(new Access()
                {
                    CreateBy = access.CreateBy,
                    CreateDate = access.CreateDate,
                    Id = Guid.NewGuid(),
                    Level = AccessLevels.View,
                    TeamId = access.TeamId,
                    UserId = access.UserId
                });
            }

        }

        private void SetCustomTokenAuthentication(List<Access> accesses)
        {
            if (accesses.Count == 0)
            {
                if (Identity.UserId == Guid.Empty && Identity.TeamId != null && Identity.TeamId != Guid.Empty)
                {
                    var access = new Access
                    {
                        TeamId = Identity.TeamId,
                        UserId = Identity.UserId,
                        Level = (AccessLevels)Identity.AccessLevel
                    };
                    accesses.Add(access);
                }
            }
        }

    }
}
