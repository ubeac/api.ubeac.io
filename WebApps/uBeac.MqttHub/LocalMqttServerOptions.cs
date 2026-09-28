using Microsoft.Extensions.Configuration;
using MQTTnet.Protocol;
using MQTTnet.Server;
using NetTools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using uBeac.HttpMqttCommon.Models;
using uBeac.HttpMqttCommon.Services;
using uBeac.Models;
using uBeac.MqttHub.Models;
using uBeac.MqttHub.Producers;
using uBeac.MqttHub.Services;

namespace uBeac.MqttHub
{
    // Due to conflict with the name of MqttServer, we have chosen LocalMqttServer as class name    
    public class LocalMqttServerOptions : MqttServerOptions
    {
        private static IMqttClientService _mqttClientService;
        private static MqttHubProducer _mqttHubProducer;
        private static IConfiguration _configuration;
        private readonly IHubService _hubService;
        private readonly TeamsModel _teamsModel;

        public LocalMqttServerOptions(IHubService hubService,
                                      IMqttClientService mqttClientService,
                                      MqttHubProducer mqttHubProducer,
                                      IConfiguration configuration)
        {
            _hubService = hubService;
            _mqttClientService = mqttClientService;
            _mqttHubProducer = mqttHubProducer;
            _configuration = configuration;
            _teamsModel = _hubService.Teams;

            DefaultEndpointOptions.Port = 1883;
            DefaultEndpointOptions.IsEnabled = true;
            ApplicationMessageInterceptor = new MqttServerApplicationMessageInterceptorDelegate(ctx => OnApplicationMessageInterceptor(ctx));
            ConnectionValidator = new MqttServerConnectionValidatorDelegate(async (validator) => { await OnConnectionValidator(validator); });
            SubscriptionInterceptor = new MqttServerSubscriptionInterceptorDelegate(ctx => OnSubscriptionInterceptor(ctx));
        }

        internal void OnApplicationMessageInterceptor(MqttApplicationMessageInterceptorContext context)
        {
            var requestDate = DateTime.UtcNow;

            if (context.ApplicationMessage.Payload.LongLength > _configuration.GetValue<long>("MaxPayloadLength") || !_mqttClientService.Get(context.ClientId, out Client client))
            {
                context.AcceptPublish = false;
                context.CloseConnection = true;
                return;
            }
                        
            var gatewayData = client.ExtractPublishedMessage(context, requestDate);
            if (gatewayData is null)
                return;

            // sending to next layer
            _mqttHubProducer.Send(gatewayData).Wait();

            // topic will be: /teamId/topic
            context.ApplicationMessage.Topic = client.TeamId + "/" + context.ApplicationMessage.Topic;
            context.AcceptPublish = true;
            context.CloseConnection = false;
        }

        internal void OnSubscriptionInterceptor(MqttSubscriptionInterceptorContext context)
        {
            if (!_mqttClientService.Get(context.ClientId, out Client client))
            {
                context.AcceptSubscription = false;
                context.CloseConnection = true;
                return;
            }

            context.TopicFilter.Topic = client.TeamId + "/" + context.TopicFilter.Topic;
            
            context.AcceptSubscription = true;
            context.CloseConnection = false;
        }

        // todo: security issue
        // if a client is connected and we change the gateway settings,
        // the client's connection will not be affected until it reconnects
        internal async Task OnConnectionValidator(MqttConnectionValidatorContext validator)
        {
            var username = validator.Username ?? string.Empty;
            var password = validator.Password ?? string.Empty;
            var clientId = validator.ClientId;

            // if clientid is guid
            if (!Guid.TryParse(clientId, out Guid gatewayId))
            {
                validator.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                return;
            }

            // fetch gateway by gateway id from cache object
            if (! _teamsModel.Gateways.TryGetValue(gatewayId, out Gateway gateway))
            {
                validator.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                return;
            }

            if (!IPAddress.TryParse(validator.Endpoint.Split(":")[0], out IPAddress clientIp))
                clientIp = IPAddress.Parse("127.0.0.1");

            if (gateway.Security != null)
            {                
                if (!PassSecurity(validator, gateway.Security, clientIp))
                    return;
            }

            var client = new Client
            {
                ClientId = validator.ClientId,
                TeamId = gateway.TeamId,
                Gateway = gateway,
                Protocol = validator.IsSecureConnection ? "MQTTS" : "MQTT",
                IPAddress = clientIp
            };

            // registering client on the server
            if (!_mqttClientService.Register(client))
            {
                validator.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                return;
            }

            validator.ReasonCode = MqttConnectReasonCode.Success;

            await Task.FromResult(0);
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

        private bool PassSecurity(MqttConnectionValidatorContext validator, GatewaySecurity security, IPAddress clientIp)
        {
            var username = validator.Username ?? string.Empty;
            var password = validator.Password ?? string.Empty;

            security.Mqtt.Username = security.Mqtt.Username ?? string.Empty;
            security.Mqtt.Password = security.Mqtt.Password ?? string.Empty;

            if (security.Mqtt != null)
            {
                // check for MQTT Enabled
                if (!security.Mqtt.Enabled)
                {
                    validator.ReasonCode = MqttConnectReasonCode.ProtocolError;
                    return false;
                }

                // check for MQTTS
                if (security.Mqtt.Tls && !validator.IsSecureConnection)
                {
                    validator.ReasonCode = MqttConnectReasonCode.NotAuthorized;
                    return false;
                }

                //check for username and password
                if (!string.IsNullOrEmpty(security.Mqtt.Username) && security.Mqtt.Username != username)
                {
                    validator.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    return false;
                }

                if (!string.IsNullOrEmpty(security.Mqtt.Password) && security.Mqtt.Password != password)
                {
                    validator.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    return false;
                }

                // checking for valid IP
                if (!HasValidIP(clientIp, security))
                {
                    validator.ReasonCode = MqttConnectReasonCode.NotAuthorized;
                    return false;
                }
            }

            return true;
        }

    }
}
