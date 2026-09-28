using System;
using System.Collections.Generic;

namespace uBeac.Models
{
    public class GatewayData
    {
        public Guid Id { get; set; }
        public string TraceId { get; set; }
        public string Body { get; set; }
        public Guid GatewayId { get; set; }
        public Guid? FloorId { get; set; }
        public Guid TeamId { get; set; }
        public Guid FirmwareId { get; set; }
        public DateTime DateTime { get; set; }
        public string Url { get; set; }
        public List<DeviceRawData> RawDevices { get; set; }
        public List<DeviceData> Devices { get; set; }
        public byte[] Bytes { get; set; }
        public List<string> Exceptions { get; set; }
        public string RequestProtocol { get; set; }
        public string RequestMethod { get; set; }

        public GatewayData()
        {
            Id = Guid.NewGuid();
            Devices = new List<DeviceData>();
            RawDevices = new List<DeviceRawData>();
            Exceptions = new List<string>();
        }
        
        public void LogException(Exception exception)
        {
            Exceptions.Add(exception.Message);
        }

        public void LogException(string message)
        {
            Exceptions.Add(message);
        }

    }
}
