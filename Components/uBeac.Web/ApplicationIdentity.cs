using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace uBeac.Web
{
    public class ApplicationIdentity : ClaimsIdentity, IApplicationIdentity
    {
        public Guid UserId { get; }
        public string Username { get; }
        public List<string> Roles { get; }

        public ApplicationIdentity(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor.HttpContext.User.Identity, httpContextAccessor.HttpContext.User.Claims)
        {
            if (httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
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
            }
        }

    }
}
