using System;
using System.Collections.Generic;

namespace uBeac.Models
{
    public class DeviceData
    {
        public Guid Id { get; set; }
        public string Uid { get; set; }
        public Guid GatewayId { get; set; }
        public DateTime DateTime { get; set; }
        public List<SensorData> Sensors { get; set; }

        public DeviceData()
        {
            Uid = string.Empty;
            DateTime = DateTime.UtcNow;
            Sensors = new List<SensorData>();
        }

        public DeviceData(DeviceRawData deviceRawData, Guid deviceId)
        {
            Id = deviceId;
            Uid = deviceRawData.Uid;
            DateTime = deviceRawData.DateTime;
            GatewayId = deviceRawData.GatewayId;
            Sensors = new List<SensorData>();
        }

    }
}
