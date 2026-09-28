using System;

namespace uBeac.Web.Middlewares
{
    public class TraceInfo
    {
        public string TraceId { get; set; }
        public DateTime StartDate { get; set; }
        public double Duration { get; set; } //milisecond
        public RequestInfo Request { get; set; }
        public ResponseInfo Response { get; set; }
        public Exception Exception { get; set; }

        public TraceInfo()
        {
        }

    }
}
