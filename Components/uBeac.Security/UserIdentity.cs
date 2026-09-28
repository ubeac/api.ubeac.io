using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace uBeac.Security
{
    public interface IUserIdentity
    {
        Guid UserId { get; }
        string Username { get; }
        List<string> Roles { get; }
        bool Authenticated { get; }
        Guid TeamId { get; }
        int AccessLevel { get; }
    }
    public class UserIdentity : IUserIdentity
    {
        public Guid UserId { get; }
        public string Username { get; }
        public List<string> Roles { get; }
        public Guid TeamId { get; }
        public bool Authenticated { get; }
        public int AccessLevel { get; }

        public UserIdentity(IHttpContextAccessor httpContextAccessor)
        {
            Authenticated = false;
            if (httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                Authenticated = true;
                var userIdString = httpContextAccessor.HttpContext.User?.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userIdString))
                {
                    userIdString = httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }

                if (!string.IsNullOrEmpty(userIdString))
                {
                    UserId = new Guid(userIdString);
                }

                Username = httpContextAccessor.HttpContext.User?.FindFirst("email")?.Value;
                if (string.IsNullOrEmpty(Username))
                {
                    Username = httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.Email)?.Value;
                }

                Roles = httpContextAccessor.HttpContext.User?.FindAll("role")?.Select(claim => claim.Value).ToList();
                if (Roles is null || Roles.Count == 0)
                {
                    Roles = httpContextAccessor.HttpContext.User?.FindAll(ClaimTypes.Role)?.Select(claim => claim.Value).ToList();
                }

                if (Guid.TryParse(httpContextAccessor.HttpContext.User?.FindFirst("TeamId")?.Value, out Guid teamId))
                    TeamId = teamId;

                if (int.TryParse(httpContextAccessor.HttpContext.User?.FindFirst("accessLevel")?.Value, out int accessLevel))
                    AccessLevel = accessLevel;
            }
        }
    }
}
