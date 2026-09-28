using System;
using System.Collections.Generic;
using System.Linq;

namespace uBeac.Models
{
    public class Sensor : BaseEntity
    {
        public string Uid { get; set; }
        public SensorTypes Type { get; set; }
        public SensorUnits Unit { get; set; }
        public string Description { get; set; }
        public SensorPrefixes Prefix { get; set; }
        public Guid DeviceId { get; set; }
        public bool Persist { get; set; }
        public List<string> Schema { get; set; }

        public Sensor(SensorRawData sensorRawData, Guid teamId, Guid deviceId)
        {
            Id = Guid.NewGuid();
            Uid = sensorRawData.SensorUid;
            Name = sensorRawData.SensorUid;
            Type = sensorRawData.Type;
            Prefix = sensorRawData.Prefix;
            Unit = sensorRawData.Unit;
            Description = string.Empty;
            CreateDate = DateTime.UtcNow;
            UpdateDate = DateTime.UtcNow;
            DeviceId = deviceId;
            TeamId = teamId;
            Schema = sensorRawData.Data.Keys.ToList();
            Persist = true;
        }

        public Sensor()
        {
            Schema = new List<string>();
            Persist = true;
        }
    }
}
