using System;

namespace uBeac.Models
{
    public class DeviceSummary
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public Guid? FloorId { get; set; }
        public Guid GatewayId { get; set; }
        public Guid DeviceId { get; set; }
        public string DeviceUid { get; set; }
        public DateTime FirstRequestDate { get; set; }
        public DateTime LastRequestDate { get; set; }
        public long RequestCount { get; set; }

        public DeviceSummary(DeviceData deviceData, Guid teamId, Guid? floorId)
        {
            Id = Guid.NewGuid();
            TeamId = teamId;
            GatewayId = deviceData.GatewayId;
            FloorId = floorId;
            DeviceId = deviceData.Id;
            DeviceUid = deviceData.Uid;
            FirstRequestDate = deviceData.DateTime;
            LastRequestDate = deviceData.DateTime;
            RequestCount = 1;
        }

        public DeviceSummary()
        {
        }
    }
}
