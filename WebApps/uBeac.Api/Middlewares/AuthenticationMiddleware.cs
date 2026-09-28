using IdentityModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using uBeac.Api.Services;

namespace uBeac.Api.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITeamService _service;

        public AuthenticationMiddleware(RequestDelegate next, ITeamService service)
        {
            _next = next;
            _service = service;
        }

        public async Task Invoke(HttpContext context)
        {
            Tuple<Guid, int> result = await _service.GetAccessAsync(context.Request);
            if (result != null)
            {
                var claims = new List<Claim>
                    {
                        new Claim(JwtClaimTypes.Id, Guid.Empty.ToString()),
                        new Claim(JwtClaimTypes.Subject, Guid.Empty.ToString()),
                        new Claim(TeamConstants.ACCESS_LEVEL, result.Item2.ToString()),
                        new Claim(TeamConstants.TEAM_ID, result.Item1.ToString())
                    };

                var identity = new ClaimsIdentity(claims, "Basic");
                var principal = new ClaimsPrincipal(identity);

                context.User = principal;
            }

            await _next(context);
        }

    }
}
