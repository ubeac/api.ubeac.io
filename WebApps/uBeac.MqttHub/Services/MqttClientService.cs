using System.Collections.Concurrent;
using uBeac.MqttHub.Models;

namespace uBeac.MqttHub.Services
{
    public interface IMqttClientService
    {
        bool Register(Client client);
        void Unregister(string clientId);
        bool Get(string clientId, out Client client);
    }

    public class MqttClientService : IMqttClientService
    {

        private readonly ConcurrentDictionary<string, Client> _registeredClients;

        public MqttClientService()
        {
            _registeredClients = new ConcurrentDictionary<string, Client>();
        }

        public bool Get(string clientId, out Client client)
        {
            return _registeredClients.TryGetValue(clientId, out client);
        }

        public bool Register(Client client)
        {
            return _registeredClients.TryAdd(client.ClientId, client);
        }

        public void Unregister(string clientId)
        {
            _registeredClients.TryRemove(clientId, out _);
        }

    }
}
