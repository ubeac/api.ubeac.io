using System;
using System.Collections.Generic;

namespace uBeac.Models
{
    public class SensorRawData
    {
        public string SensorUid { get; set; }
        public DateTime DateTime { get; set; }
        public SensorTypes Type { get; set; }
        public SensorUnits Unit { get; set; }
        public SensorPrefixes Prefix { get; set; }
        public Dictionary<string, decimal> Data { get; set; }

        public SensorRawData()
        {
            Data = new Dictionary<string, decimal>();
            Type = 0;
            Prefix = 0;
            Unit = 0;
            DateTime = DateTime.UtcNow;
        }

        public SensorRawData(string Uid, DateTime dateTime, SensorTypes sensorType, SensorUnits sensorUnit, SensorPrefixes sensorPrefixe, Dictionary<string, decimal> data)
        {
            SensorUid = Uid;
            Data = data;
            Type = sensorType;
            Prefix = sensorPrefixe;
            Unit = sensorUnit;
            DateTime = dateTime;
        }

        public SensorRawData(string Uid, DateTime dateTime, SensorTypes sensorType, SensorUnits sensorUnit, SensorPrefixes sensorPrefixe, decimal value)
        {
            SensorUid = Uid;
            Data = new Dictionary<string, decimal> { { "value", value } };
            Type = sensorType;
            Prefix = sensorPrefixe;
            Unit = sensorUnit;
            DateTime = dateTime;
        }
    }
}
