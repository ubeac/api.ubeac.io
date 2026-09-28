using System;
using System.Collections.Generic;

namespace uBeac.Models
{
    public class Gateway : BaseEntity
    {
        public Guid FirmwareId { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime? LastRequestDate { get; set; }
        public long RequestCount { get; set; }
        public Guid? FloorId { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public string Processor { get; set; }
        public ProcessorLanguage Language { get; set; }
        public GatewaySecurity Security { get; set; }

        public Gateway()
        {
            RequestCount = 0;
            X = 50;
            Y = 50;
            Security = new GatewaySecurity();
            Language = ProcessorLanguage.NA;
            Security = new GatewaySecurity();
        }
    }

    public class GatewaySecurity
    {
        public GatewaySecurityHttp Http { get; set; }
        public GatewaySecurityMqtt Mqtt { get; set; }
        public GatewaySecurityIpRestriction IpRestriction { get; set; }

        public GatewaySecurity()
        {
            Http = new GatewaySecurityHttp();
            Mqtt = new GatewaySecurityMqtt();
            IpRestriction = new GatewaySecurityIpRestriction();
        }
    }

    public class GatewaySecurityHttp
    {
        public bool Ssl { get; set; }
        public bool Enabled { get; set; }
        public GatewaySecurityHttpHeaders Headers { get; set; }

        public GatewaySecurityHttp()
        {
            Ssl = false;
            Enabled = true;
            Headers = new GatewaySecurityHttpHeaders();
        }
    }

    public class GatewaySecurityHttpHeaders : Dictionary<string, string>
    {
    }

    public class GatewaySecurityMqtt
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Tls { get; set; }
        public bool Enabled { get; set; }

        public GatewaySecurityMqtt()
        {
            Tls = false;
            Enabled = true;
        }
    }

    public class GatewaySecurityIpRestriction
    {
        public List<string> AllowedIps { get; set; }
        public List<string> DeniedIps { get; set; }

        public GatewaySecurityIpRestriction()
        {
            AllowedIps = new List<string>();
            DeniedIps = new List<string>();
        }
    }

}