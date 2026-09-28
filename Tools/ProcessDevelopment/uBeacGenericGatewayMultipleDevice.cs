using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class uBeacGenericGatewayMultipleDevice : IProcessor
    {
        public void Process(GatewayData gatewayData)
        {
            if (string.IsNullOrEmpty(gatewayData.Body))
                return;

            try
            {
                var rawList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(gatewayData.Body);
                new BaseProcessorTemplate();

                foreach (var rawDic in rawList)
                {
                    BaseProcessorTemplate.ExtractRawDevices(gatewayData, rawDic);
                }
            }
            catch (Exception ex)
            {
                gatewayData.Exceptions.Add(ex.Message);
            }
        }
    }
}
