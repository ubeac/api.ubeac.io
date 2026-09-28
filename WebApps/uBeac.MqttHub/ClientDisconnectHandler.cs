using MQTTnet.Server;
using System.Threading.Tasks;
using uBeac.MqttHub.Services;

namespace uBeac.MqttHub
{
    public class ClientDisconnectHandler : IMqttServerClientDisconnectedHandler
    {
        private readonly IMqttClientService _mqttClientService;

        public ClientDisconnectHandler(IMqttClientService mqttClientService)
        {
            _mqttClientService = mqttClientService;
        }

        public Task HandleClientDisconnectedAsync(MqttServerClientDisconnectedEventArgs eventArgs)
        {
            _mqttClientService.Unregister(eventArgs.ClientId);
            return Task.FromResult(0);
        }
    }
}
