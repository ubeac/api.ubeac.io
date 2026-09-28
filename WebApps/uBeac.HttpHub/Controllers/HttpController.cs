using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uBeac.HttpHub.Producers;
using uBeac.Models;

namespace uBeac.HttpHub.Controllers
{
    // todo: we need debug infor fo the IoT developers (if request contains a querystring: debug we should response more info on the result
    // todo: add cancellationtoken to methods

    public class HttpController
    {
        private readonly HubProducer _hubProducer;
        private readonly HttpContext _httpContext;
        private readonly DateTime _requestDate;
        private GatewayData _gatewayData;
        private readonly Gateway _gateway;
        private readonly string[] _sensorValueKeys = new[] { Constants.VALUE, Constants.DATA };

        public HttpController(HubProducer hubProducer, IHttpContextAccessor contextAccessor)
        {
            _hubProducer = hubProducer;
            _requestDate = DateTime.UtcNow;
            _httpContext = contextAccessor.HttpContext;
            _gateway = (Gateway)_httpContext.Items[Constants.GATEWAY];
        }

        [Route("{gatewayUrl}")]
        [AcceptVerbs("POST", "PUT", "PATCH", "GET")]
        public async Task<IActionResult> GatewayReceived(string gatewayUrl)
        {
            // Note: If user sends sensorData we do not accept payload and make gatewayData body empty
            _gatewayData = GetGatewayData(true);
            return await FinalizeRequest();
        }

        // {namespace}.hub.ubeac.io/{gatewayUrl}/devices/{deviceUid}/sensors/{sensorUid}?value=12
        // {namespace}.hub.ubeac.io/{gatewayUrl}/devices/{deviceUid}/sensors/{sensorUid}?x=1&y=13&z=15
        [Route("{gatewayUrl}/devices/{deviceUid}/sensors/{sensorUid}")]
        [AcceptVerbs("POST", "PUT", "PATCH", "GET")]
        public async Task<IActionResult> SensorReceived(string gatewayUrl, string deviceUid, string sensorUid)
        {
            // there is no querystring
            if (_httpContext.Request.Query.Count == 0)
                return GetActionResult(StatusCodes.Status400BadRequest);

            var deviceRawData = new DeviceRawData
            {
                DateTime = _requestDate,
                GatewayId = _gateway.Id,
                IsValid = true,
                Uid = deviceUid
            };

            SensorTypes type = SensorTypes.NA;
            SensorPrefixes prefix = SensorPrefixes.One;
            SensorUnits unit = SensorUnits.NA;

            var sensorValueDic = new Dictionary<string, decimal>();

            foreach (var item in _httpContext.Request.Query)
            {

                if (item.Key.ToLower() == Constants.Type)
                {
                    var tempValue = (SensorTypes)Enum.Parse(typeof(SensorTypes), item.Value);
                    if (Enum.IsDefined(typeof(SensorTypes), tempValue))
                        type = tempValue;

                    continue;
                }

                if (item.Key.ToLower() == Constants.Unit)
                {
                    var tempValue = (SensorUnits)Enum.Parse(typeof(SensorUnits), item.Value);
                    if (Enum.IsDefined(typeof(SensorUnits), tempValue))
                        unit = tempValue;

                    continue;
                }

                if (item.Key.ToLower() == Constants.Prefix)
                {
                    var tempValue = (SensorPrefixes)Enum.Parse(typeof(SensorPrefixes), item.Value);
                    if (Enum.IsDefined(typeof(SensorPrefixes), tempValue))
                        prefix = tempValue;

                    continue;
                }

                if (decimal.TryParse(item.Value, out decimal value))
                    sensorValueDic.Add(item.Key, value);
            }

            // there is no value
            if (sensorValueDic.Count == 0)
                return GetActionResult(StatusCodes.Status400BadRequest);

            // if there is one value
            if (sensorValueDic.Count == 1)
            {
                if (!_sensorValueKeys.Contains(sensorValueDic.Keys.First().ToLower()))
                    return GetActionResult(StatusCodes.Status400BadRequest);

                deviceRawData.Sensors.Add(new SensorRawData(sensorUid, _requestDate, type, unit, prefix, sensorValueDic.Values.First()));
            }
            else
            {
                deviceRawData.Sensors.Add(new SensorRawData(sensorUid, _requestDate, type, unit, prefix, sensorValueDic));
            }

            // Note: If user sends sensorData we do not accept payload and make gatewayData body empty
            _gatewayData = GetGatewayData(false);
            _gatewayData.RawDevices.Add(deviceRawData);

            return await FinalizeRequest();
        }

        private async Task<IActionResult> FinalizeRequest()
        {
            // sending to next layer
            await _hubProducer.Send(_gatewayData);

            return GetActionResult(StatusCodes.Status200OK);
        }

        private IActionResult GetActionResult(int statusCode)
        {
            _httpContext.Response.StatusCode = statusCode;
            return new StatusCodeResult(statusCode);
        }

        private GatewayData GetGatewayData(bool acceptPayload)
        {
            var result = new GatewayData
            {
                DateTime = _requestDate,
                RequestMethod = _httpContext.Request.Method,
                RequestProtocol = _httpContext.Request.Protocol,
                TraceId = _httpContext.TraceIdentifier,
                GatewayId = _gateway.Id,
                FirmwareId = _gateway.FirmwareId,
                TeamId = _gateway.TeamId,
                FloorId = _gateway.FloorId,
                Url = _gateway.Url
            };

            if (acceptPayload)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    _httpContext.Request.Body.CopyTo(memoryStream);
                    result.Bytes = memoryStream.ToArray();
                    memoryStream.Position = 0;
                    using (StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8))
                    {
                        result.Body = reader.ReadToEnd();
                    }
                }
            }

            return result;

        }

    }

}
