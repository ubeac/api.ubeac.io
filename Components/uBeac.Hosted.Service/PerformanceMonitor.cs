using System;
using System.Timers;

namespace uBeac.Hosted.Service
{
    public class PerformanceMonitor
    {
        private Timer _timer;
        public long Counter { get; set; }
        public int MilliSeconds { get; set; }
        public bool Active { get; set; }

        public PerformanceMonitor()
        {

        }

        public void Start()
        {
            if (Active)
            {
                _timer = new Timer(MilliSeconds);
                _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                _timer.Start();
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (Active)
            {
                Console.WriteLine("Processed count:" + Counter.ToString());
                Counter = 0;
            }
        }

        public void Increment()
        {
            Counter += 1;
        }

    }
}
