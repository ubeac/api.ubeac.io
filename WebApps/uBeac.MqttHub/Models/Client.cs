using System;
using System.Net;
using uBeac.Models;

namespace uBeac.MqttHub.Models
{
    public class Client
    {
        public string ClientId { get; set; }
        public Guid TeamId { get; set; }
        public Gateway Gateway { get; set; }
        public string Protocol { get; set; }
        public IPAddress IPAddress{ get; set; }
    }
}
