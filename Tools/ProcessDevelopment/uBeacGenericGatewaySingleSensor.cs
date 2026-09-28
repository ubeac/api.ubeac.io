using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class uBeacGenericGatewaySingleSensor : IProcessor
    {
        public void Process(GatewayData gatewayData)
        {
            if (string.IsNullOrEmpty(gatewayData.Body))
                return;

            try
            {
                var rawDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(gatewayData.Body);
                var sensorRawData = new SensorRawData();
                var deviceRawData = new DeviceRawData { IsValid = true, GatewayId = gatewayData.GatewayId };
                deviceRawData.Sensors.Add(sensorRawData);
                new BaseProcessorTemplate();

                BaseProcessorTemplate.ProcessSensorData(gatewayData, rawDic, deviceRawData, sensorRawData, false);

                if (sensorRawData.Data.Count > 0)
                {
                    deviceRawData.DateTime = sensorRawData.DateTime;
                    gatewayData.RawDevices.Add(deviceRawData);
                }
            }
            catch (Exception ex)
            {
                gatewayData.Exceptions.Add(ex.Message);
            }
        }
    }
}
