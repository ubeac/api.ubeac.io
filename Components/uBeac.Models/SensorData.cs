using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using uBeac.Serialization;

namespace uBeac.Models
{
    public class SensorData
    {
        public Guid SensorId { get; set; }        
        public Guid GatewayId { get; set; }
        public DateTime DateTime { get; set; }
     
        [JsonConverter(typeof(CustomDecimalDictionarySerializer))]
        public Dictionary<string, decimal> Data { get; set; }

        public SensorData()
        {
            Data = new Dictionary<string, decimal>();
        }

        public SensorData(SensorRawData sensorRawData, Guid sensorId, Guid gatewayId) 
        {
            SensorId = sensorId;
            GatewayId = gatewayId;
            DateTime = sensorRawData.DateTime;
            if (sensorRawData.Data is null)
                Data = new Dictionary<string, decimal>();
            else
                Data = sensorRawData.Data;
        }

    }
}
