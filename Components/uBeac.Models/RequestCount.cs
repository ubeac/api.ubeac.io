using System;

namespace uBeac.Models
{
    public class RequestCount
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public RequestType Type { get; set; }
        public object Value { get; set; }
        public DateTime DateTime { get; set; }
    }

    public enum RequestType
    {
        GatewayDataRequest = 1,
        DeviceDataRequest = 2
    }
}
