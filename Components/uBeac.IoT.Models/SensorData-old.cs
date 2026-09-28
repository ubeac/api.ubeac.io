//using MessagePack;
//using System;

//namespace uBeac.IoT.Models
//{
//    [Union(0, typeof(DecimalData))]
//    [Union(1, typeof(GPSData))]
//    [Union(2, typeof(BeaconData))]
//    [Union(3, typeof(AccelerationData))]
//    public abstract class SensorData
//    {
//        public Guid Id { get; set; }
//        public string TagUid { get; set; }
//        public DateTime DateTime { get; set; }
//        //public Guid EndpointDataId { get; set; }
//        public virtual SensorTypes Type { get; set; }

//        public SensorData()
//        {
//            Id = Guid.NewGuid();
//        }
//    }
//}
