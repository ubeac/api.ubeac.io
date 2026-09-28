/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System;
using System.Threading.Tasks;

namespace uBeac.WebSocket
{
    public interface ISocket
    {
        Task SendPrivateAsync<T>(string clientId, T message);
        Task SendPublicAsync<T>(T message);
        Task SendToGroupAsync<T>(string groupId, T message);
        Task OnReceiveAsync(string clientId, object data);
        Task OnJoinAsync(string clientId, string groupId);
        Task OnLeaveAsync(string clientId, string groupId);
        Task OnConnectedAsync(string clientId);
        Task OnDisconnectedAsync(string clientId, Exception exception);
    }
}
