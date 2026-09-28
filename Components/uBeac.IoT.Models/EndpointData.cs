//using System;
//using System.Collections.Generic;
//using uBeac.Models;

//namespace uBeac.IoT.Models
//{
//    //[MessagePackObject(keyAsPropertyName: true)]
//    public class EndpointData
//    {
//        public Guid Id { get; set; }
//        public string TraceId { get; set; }
//        public string Body { get; set; }
//        public Guid EndpointId { get; set; }
//        public Guid? FloorId { get; set; }
//        public Guid OrganizationId { get; set; }
//        public Guid GatewayFirmwareId { get; set; }
//        public DateTime DateTime { get; set; }
//        public string Url { get; set; }
//        public List<DeviceData> Devices { get; set; }
//        public byte[] Bytes { get; set; }
//        public List<Exception> Exceptions { get; set; }

//        public EndpointData()
//        {
//            Id = Guid.NewGuid();
//            Devices = new List<DeviceData>();
//            Exceptions = new List<Exception>();
//        }
//    }
//}
