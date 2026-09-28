using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace uBeac.Models
{
    public class DeviceRawData 
    {
        public string Uid { get; set; }
        [BsonIgnore]
        public Guid GatewayId { get; set; }
        public DateTime DateTime { get; set; }
        public List<SensorRawData> Sensors { get; set; }
        public bool IsValid { get; set; }        

        public DeviceRawData()
        {
            Uid = string.Empty;
            DateTime = DateTime.UtcNow;
            Sensors = new List<SensorRawData>();
            IsValid = false;
        }
    }
}
