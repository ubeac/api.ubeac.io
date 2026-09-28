//using System;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace uBeac.Models
//{
//    public class DeviceSummary
//    {
//        public DeviceSummaryKey Id { get; set; }
//        public DateTime FirstRequestDate { get; set; }
//        public DateTime LastRequestDate { get; set; }
//        public SensorData LastData { get; set; }
//        public double RequestCount { get; set; }
//    }

//    public class DeviceSummaryKey
//    {
//        [Column(Order = 1)]
//        public string DeviceUid { get; set; }
//        [Column(Order = 2)]
//        public string SensorUid { get; set; }
//        [Column(Order = 3)]
//        public int SensorType { get; set; }
//    }
//}
