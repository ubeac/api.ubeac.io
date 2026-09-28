using Microsoft.AspNetCore.Http;
using NetTools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using uBeac.Models;

namespace uBeac.HttpHub.Middlewares
{
    public class GatewaySecurityMiddleware
    {
        private readonly RequestDelegate _next;
        public GatewaySecurityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var teamId = (Guid)context.Items[Constants.TEAM_ID];
            var gateway = (Gateway)context.Items[Constants.GATEWAY];

            if (gateway.Security == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var security = gateway.Security;

            if (security.Http != null)
            {
                // check for HTTP enabled 
                if (!security.Http.Enabled)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }

                // check for HTTP/HTTPS 
                if (security.Http.Ssl && !context.Request.IsHttps)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                //check for request headers
                if (!HasValidHttpHeaders(context.Request, security))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            // check for IP
            if (!HasValidIP(context.Connection.RemoteIpAddress, security))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await _next(context);

        }

        private bool HasValidHttpHeaders(HttpRequest httpRequest, GatewaySecurity security)
        {
            try
            {
                if (security.Http.Headers is null || security.Http.Headers.Count == 0)
                    return true;

                foreach (var item in security.Http.Headers)
                {
                    if (!httpRequest.Headers.ContainsKey(item.Key) || httpRequest.Headers[item.Key][0] != item.Value)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool HasValidIP(IPAddress ipAddress, GatewaySecurity security)
        {
            try
            {

                if (security.IpRestriction is null)
                    return true;

                var ipRestriction = security.IpRestriction;

                if (ipRestriction.AllowedIps is null)
                    ipRestriction.AllowedIps = new List<string>();

                if (ipRestriction.DeniedIps is null)
                    ipRestriction.DeniedIps = new List<string>();


                foreach (var allowedIp in ipRestriction.AllowedIps)
                {
                    var rangeIp = IPAddressRange.Parse(allowedIp);
                    if (rangeIp.Contains(ipAddress))
                        return true;
                }

                foreach (var deniedIp in ipRestriction.DeniedIps)
                {
                    var rangeIp = IPAddressRange.Parse(deniedIp);
                    if (rangeIp.Contains(ipAddress))
                        return false;
                }

                if (ipRestriction.AllowedIps.Count > 0)
                    return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
