using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOITScheduleICSGenerator
{
    class CalEvent
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsRecurring { get; set; }
        public string ReccurenceConditions { get; set; }
        public string Reminder { get; set; }
        public bool Busy { get; set; }

        public CalEvent() { }

        
    }
}
