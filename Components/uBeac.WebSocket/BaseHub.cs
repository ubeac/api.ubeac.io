/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Security;

namespace uBeac.WebSocket
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BaseHub : Hub
    {
        ISocket _socket;
        private readonly ILogger<BaseHub> _logger;
        private readonly ISecurityContext _securityContext;

        public BaseHub(ISocket socket, ILogger<BaseHub> logger, ISecurityContext securityContext)
        {
            _socket = socket;
            _logger = logger;
            _securityContext = securityContext;
        }

        public async Task SendToGroup(string groupId, object data)
        {
            await Clients.Group(groupId).SendAsync("onGroupData", groupId, data);
        }

        public async Task SendToAll(object data)
        {
            await Clients.All.SendAsync("onPublicData", data);
        }

        public async Task SendToClient(string clientId, object data)
        {
            await Clients.Client(clientId).SendAsync("onPrivateData", data);
        }

        public async Task Send(object data)
        {
            await _socket.OnReceiveAsync(Context.ConnectionId, data);
        }

        public async Task Join(string groupId)
        {
            var teamStr = groupId.Split('/')[1];
            if (!_securityContext.HasAccess<Team>(AccessLevels.View, Guid.Parse(teamStr)))
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            await _socket.OnJoinAsync(Context.ConnectionId, groupId);
            _logger.LogInformation("User with connectionId: {ConnectionId} joind to the group: {@GroupId} at {@Timestamp}", Context.ConnectionId, groupId, DateTime.UtcNow);
        }

        public async Task Leave(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
            await _socket.OnLeaveAsync(Context.ConnectionId, groupId);
            _logger.LogInformation("User with connectionId: {ConnectionId} left the group: {@GroupId} at {@Timestamp}", Context.ConnectionId, groupId, DateTime.UtcNow);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await _socket.OnConnectedAsync(Context.ConnectionId);
            _logger.LogInformation("User with connectionId: {@ConnectionId} was connected to socket at {@Timestamp}", Context.ConnectionId, DateTime.UtcNow);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            await _socket.OnDisconnectedAsync(Context.ConnectionId, exception);
            _logger.LogInformation("User with connectionId: {@ConnectionId} disconnected from socket at {@Timestamp}", Context.ConnectionId, DateTime.UtcNow);
        }

    }
}
