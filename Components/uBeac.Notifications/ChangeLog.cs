using System;

namespace uBeac.Notifications
{
    public class ChangeLog
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public object Value { get; set; }
        public ActionTypes Action { get; set; }
        public Guid TeamId { get; set; }
        public string Type { get; set; }

        public ChangeLog()
        {
            DateTime = DateTime.UtcNow;
        }        
    }
}
