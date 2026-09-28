using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet.AspNetCore;
using MQTTnet.Diagnostics;
using MQTTnet.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uBeac.HttpMqttCommon.Services;
using uBeac.Models;
using uBeac.MqttHub.Models;
using uBeac.MqttHub.Producers;
using uBeac.MqttHub.Services;

namespace uBeac.MqttHub
{
    public static class Extensions
    {
        private static readonly string[] _sensorValueKeys = new[] { Constants.VALUE, Constants.DATA };
        public static IServiceCollection AddMqttLocalServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMqttServerOptions>(s =>
            {
                var mqttservice = s.GetService<IHubService>();
                var mqttClientService = s.GetService<IMqttClientService>();
                var mqttHubProducer = s.GetService<MqttHubProducer>();

                return new LocalMqttServerOptions(mqttservice, mqttClientService, mqttHubProducer, configuration);
            });

            var logger = new MqttNetLogger();
            var childLogger = logger.CreateChildLogger();

            services.AddSingleton<IMqttNetLogger>(logger);
            services.AddSingleton(childLogger);
            services.AddSingleton<MqttHostedServer>();
            services.AddSingleton<IHostedService>(s => s.GetService<MqttHostedServer>());
            services.AddSingleton<IMqttServer>(s => s.GetService<MqttHostedServer>());

            services.AddMqttTcpServerAdapter()
                .AddMqttWebSocketServerAdapter();

            services.AddSingleton<IMqttClientService, MqttClientService>();
            services.AddSingleton<IMqttServerClientDisconnectedHandler, ClientDisconnectHandler>();

            return services;
        }

        public static GatewayData ExtractPublishedMessage(this Client client, MqttApplicationMessageInterceptorContext context, DateTime requestDate)
        {
            // checking if the topic is the same as gateway url, create gateway data and publish it
            var gatewayData = new GatewayData();
            var splittedTopic = context.ApplicationMessage.Topic.Split('/');

            try
            {
                // checking for empty topic
                if (splittedTopic.Length == 0)
                    return null;

                // checking for the gatewayUrl:
                // if the gatewayUrl does not equal to the first segment of topic, we will not process the payload
                if (client.Gateway.Url != splittedTopic[0])
                    return null;

                // checking if the topic is the same as gateway url, create gateway data and publish it
                if (splittedTopic.Length == 1)
                    return GetGatewayData(client, context.ApplicationMessage.Payload, requestDate, true);


                // validating puclished sensor data as multipart topic
                // {namespace}.hub.ubeac.io/{gatewayUrl}/devices/{deviceUid}/sensors/{sensorUid} and data will be in payload
                if (splittedTopic.Length != 5 || splittedTopic[1].ToLower() != Constants.DEVICES ||
                            splittedTopic[3].ToLower() != Constants.SENSORS || context.ApplicationMessage.Payload.Length == 0)
                    return null;

                var sensorValueDic = new Dictionary<string, decimal>();
                var deviceUid = splittedTopic[2];
                var sensorUid = splittedTopic[4];
                var body = Encoding.UTF8.GetString(context.ApplicationMessage.Payload);

                // checking the payload if is decimal
                if (decimal.TryParse(body, out decimal rawData))
                {
                    sensorValueDic.Add(Constants.VALUE, rawData);
                }
                else
                {

                    sensorValueDic = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(body);

                    // if there is only one item in the json file with different name than "value", "data"
                    // we will not accept that
                    if (sensorValueDic.Count == 1 && !_sensorValueKeys.Contains(sensorValueDic.Keys.First().ToLower()))
                        return null;
                }

                gatewayData = GetGatewayData(client, context.ApplicationMessage.Payload, requestDate, false);
                gatewayData.RawDevices.Add(GetRawDevice(deviceUid, sensorUid, sensorValueDic, requestDate, gatewayData.GatewayId));

                return gatewayData;

            }
            catch (Exception)
            {
                return null;
            }

        }

        private static GatewayData GetGatewayData(Client client, byte[] payload, DateTime requestDate, bool acceptPayload)
        {
            var gateway = client.Gateway;

            var gatewayData = new GatewayData
            {
                DateTime = requestDate,
                RequestMethod = string.Empty,
                RequestProtocol = client.Protocol,
                TraceId = Guid.NewGuid().ToString(),
                GatewayId = gateway.Id,
                FirmwareId = gateway.FirmwareId,
                TeamId = gateway.TeamId,
                FloorId = gateway.FloorId,
                Url = gateway.Url
            };

            if (acceptPayload)
            {
                gatewayData.Body = Encoding.UTF8.GetString(payload);
                //gatewayData.Bytes = payload;
            }

            return gatewayData;
        }

        private static DeviceRawData GetRawDevice(string deviceUid, string sensorUid, Dictionary<string, decimal> sensorValue, DateTime requestDate, Guid gatewayId)
        {
            return new DeviceRawData
            {
                DateTime = requestDate,
                GatewayId = gatewayId,
                IsValid = true,
                Uid = deviceUid,
                Sensors = new List<SensorRawData> {
                    new SensorRawData{
                        Data = sensorValue,
                        DateTime = requestDate,
                        SensorUid = sensorUid
                    }
                }
            };
        }

    }
}
