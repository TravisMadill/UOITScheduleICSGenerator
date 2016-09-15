using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOITScheduleICSGenerator
{
    class CalFile
    {
        public CalFile() { }

        public static string CreateICSFileContents(List<CalEvent> events)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("PRODID:-//Google Inc//Google Calendar 70.9054//EN");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine("METHOD:PUBLISH");
            sb.AppendLine("X-WR-CALNAME:School");
            sb.AppendLine("X-WR-TIMEZONE:America/New_York");
            sb.AppendLine("BEGIN:VTIMEZONE");
            sb.AppendLine("TZID:America/New_York");
            sb.AppendLine("X-LIC-LOCATION:America/New_York");
            sb.AppendLine("BEGIN:DAYLIGHT");
            sb.AppendLine("TZOFFSETFROM:-0500");
            sb.AppendLine("TZOFFSETTO:-0400");
            sb.AppendLine("TZNAME:EDT");
            sb.AppendLine("DTSTART:19700308T020000");
            sb.AppendLine("RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=2SU");
            sb.AppendLine("END:DAYLIGHT");
            sb.AppendLine("BEGIN:STANDARD");
            sb.AppendLine("TZOFFSETFROM:-0400");
            sb.AppendLine("TZOFFSETTO:-0500");
            sb.AppendLine("TZNAME:EST");
            sb.AppendLine("DTSTART:19701101T020000");
            sb.AppendLine("RRULE:FREQ=YEARLY;BYMONTH=11;BYDAY=1SU");
            sb.AppendLine("END:STANDARD");
            sb.AppendLine("END:VTIMEZONE");
            foreach(CalEvent e in events)
                sb.Append(e.GetVEventString());
            sb.Append("END:VCALENDAR");
            return sb.ToString();
        }
    }
}
