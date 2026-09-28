/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uBeac.Serialization;

namespace uBeac.WebSocket
{
    public class BaseSocket : ISocket
    {
        IHubContext<BaseHub> _hub;

        public BaseSocket(IHubContext<BaseHub> hub)
        {
            _hub = hub;
        }

        private string Serialize(object data)
        {
            // todo: it is better to use uBeac.Serilization
            return JsonConvert.SerializeObject(data,
                new JsonSerializerSettings
                {
                    // todo: do we need to set this setting
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = new List<JsonConverter> { new CustomDecimalDictionarySerializer(), new CustomDictionarySerializer() }
                });
        }

        public async Task SendPrivateAsync<T>(string clientId, T message)
        {
            var _message = Serialize(message);
            await _hub.Clients.Client(clientId).SendAsync("onPrivateData", message);
        }

        public async Task SendPublicAsync<T>(T message)
        {
            var _message = Serialize(message);
            await _hub.Clients.All.SendAsync("onPublicData", _message);
        }

        public async Task SendToGroupAsync<T>(string groupId, T message)
        {
            var _message = Serialize(message);
            await _hub.Clients.Group(groupId).SendAsync("onGroupData", groupId, _message);
        }

        public async Task OnReceiveAsync(string clientId, object data)
        {
            await Task.FromResult(0);
        }

        public async Task OnJoinAsync(string clientId, string groupId)
        {
            await _hub.Clients.Group(groupId).SendAsync("onJoin", clientId);
        }

        public async Task OnLeaveAsync(string clientId, string groupId)
        {
            await _hub.Clients.Group(groupId).SendAsync("onLeave", clientId);
            await _hub.Clients.Client(clientId).SendAsync("onLeave", clientId);
        }

        public async Task OnConnectedAsync(string clientId)
        {
            await _hub.Clients.Client(clientId).SendAsync("onConnect", clientId);
        }

        public async Task OnDisconnectedAsync(string clientId, Exception exception)
        {
            await Task.FromResult(0);
        }
    }
}
