using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace uBeac.Models
{
    public class Device : BaseEntity
    {
        public string Uid { get; set; }
        public string Description { get; set; }
        public Guid? FloorId { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }

        [BsonIgnore]
        public List<Sensor> Sensors { get; set; }

        public Device(DeviceRawData deviceRawData, Guid teamId)
        {
            Id = Guid.NewGuid();
            Name = deviceRawData.Uid;
            Uid = deviceRawData.Uid;
            CreateDate = DateTime.UtcNow;
            UpdateDate = DateTime.UtcNow;
            TeamId = teamId;
            Description = string.Empty;
            UpdateBy = Guid.Empty;
            CreateBy = Guid.Empty;
            Sensors = new List<Sensor>();
            X = 50;
            Y = 50;
        }

        public Device()
        {
            Sensors = new List<Sensor>();
        }
    }
}
