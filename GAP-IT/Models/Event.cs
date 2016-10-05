using System;
using System.Collections.Generic;

namespace GAP_IT.Models
{
    public class EventEntry
    {
        public string EventId { get; set; }
        public int EventSet { get; set; }
        public DateTime Time { get; set; }
        public User User { get; set; }
        public Package Package { get; set; }
        public virtual List<Timings> Timings { get; set; }
    }
}
