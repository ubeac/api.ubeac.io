//using System;
//using System.Collections.Generic;
//using uBeac.Models;

//namespace uBeac.IoT.Models
//{
//    //[MessagePackObject(keyAsPropertyName: true)]
//    public class EndpointRawData
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
//        //public List<TagData> Tags { get; set; }
//        public List<DeviceRawData> Devices { get; set; }
//        public byte[] Bytes { get; set; }
//        public List<Exception> Exceptions { get; set; }

//        public EndpointRawData()
//        {
//            Id = Guid.NewGuid();
//            //Tags = new List<TagData>();
//            Devices = new List<DeviceRawData>();
//            Exceptions = new List<Exception>();
//        }
//    }
//}
