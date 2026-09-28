//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using uBeac.Api.Services;
//using uBeac.Models;

//namespace uBeac.Api
//{
//    public interface ISecurityContext
//    {
//        IUserIdentity Identity { get; }
//        bool HasAccess<TEntity>(AccessLevels accessLevel, Guid entityId);
//        List<Guid> GetAccessible<TEntity>(AccessLevels accessLevel);
//    }
//    public class SecurityContext : ISecurityContext
//    {
//        private readonly List<Access> _accesses;
//        public SecurityContext(IHttpContextAccessor httpContextAccessor, IAccessService accessService)
//        {
//            // setting default empty access list
//            _accesses = new List<Access>();

//            // preparing curreny context's identity
//            Identity = new UserIdentity(httpContextAccessor);

//            if (Identity.Authenticated)
//            {
//                // fetching all accesses for current user
//                _accesses = accessService.GetAllAsync(Identity.UserId).Result;
//            }

//            // if user has admin permission to an entity, we should add view permission as well on the fly
//            var adminAccesses = _accesses.Where(x => x.Level == AccessLevels.Admin).ToList();
//            foreach (var access in adminAccesses)
//            {
//                _accesses.Add(new Access() { Level = AccessLevels.View, TeamId = access.TeamId });
//            }

//        }

//        public IUserIdentity Identity { get; }

//        public bool HasAccess<TEntity>(AccessLevels accessLevel, Guid entityId)
//        {
//            // if the logged in user is system administrator (member of uBeac or Momentaj)
//            if (Identity.Roles != null && Identity.Roles.Contains("ADMINS"))
//                return true;

//            var accessLevels = GetOverridenAccessLevels(accessLevel);
//            return _accesses.Where(x => accessLevels.Contains(x.Level) && x.TeamId == entityId).Count() > 0;
//        }

//        public List<Guid> GetAccessible<TEntity>(AccessLevels accessLevel)
//        {
//            var accessLevels = GetOverridenAccessLevels(accessLevel);
//            return _accesses.Where(x => accessLevels.Contains(x.Level)).Select(x => x.TeamId).Distinct().ToList();
//        }

//        private List<AccessLevels> GetOverridenAccessLevels(AccessLevels accessLevel)
//        {
//            var accessLevels = new List<AccessLevels>(new[] { accessLevel });
//            if (accessLevel == AccessLevels.Admin)
//                accessLevels.Add(AccessLevels.View);

//            return accessLevels;
//        }

//    }
//}
