using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class uBeacGenericGatewayMultipleSensor : IProcessor
    {
        public void Process(GatewayData gatewayData)
        {
            if (string.IsNullOrEmpty(gatewayData.Body))
                return;

            try
            {
                var rawDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(gatewayData.Body);
                new BaseProcessorTemplate();

                BaseProcessorTemplate.ExtractRawDevices(gatewayData, rawDic);
            }
            catch (Exception ex)
            {
                gatewayData.Exceptions.Add(ex.Message);
            }
        }
    }
}
