using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOITScheduleICSGenerator
{
    class CalEvent
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurrenceConditions { get; set; }
        public string Reminder { get; set; }
        public bool Busy { get; set; }

        public CalEvent() { }

        public static CalEvent classAsCalEvent(Class c)
        {
            CalEvent ce = new CalEvent();
            string title, desc, loc, lec, tut, lab, reminder = null;
            bool busy;
            try
            {
                using(BinaryReader br = new BinaryReader(File.Open(Form_Format.settingFilePath, FileMode.Open)))
                {
                    title = br.ReadString();
                    desc = br.ReadString();
                    loc = br.ReadString();
                    lec = br.ReadString();
                    tut = br.ReadString();
                    lab = br.ReadString();
                    busy = br.ReadBoolean();
                    if(br.ReadBoolean())
                    {
                        reminder = ((int)br.ReadDecimal()).ToString();
                        reminder += " ";
                        reminder += br.ReadInt32().ToString();
                    }
                }
            }
            catch (IOException)
            {
                title = "<ClassType>: <ClassName> (<CRN>)";
                desc = "Course code: <CourseCode>" + Environment.NewLine + "CRN: <CRN>";
                loc = "<Location>";
                lec = "Lec.";
                tut = "Tut.";
                lab = "Lab";
                reminder = "20 0";
                busy = true;
            }
            switch (c.Weekday)
            {
                case "M":
                    c.Weekday = "MO";
                    break;
                case "T":
                    c.Weekday = "TU";
                    break;
                case "W":
                    c.Weekday = "WE";
                    break;
                case "R":
                    c.Weekday = "TH";
                    break;
                case "F":
                    c.Weekday = "FR";
                    break;
            }

            ce.Name = replaceXMLTags(title, c, lec, tut, lab);
            ce.Description = replaceXMLTags(desc, c, lec, tut, lab);
            ce.Location = replaceXMLTags(loc, c, lec, tut, lab);
            ce.StartTime = DateTime.Parse(c.StartDate + " " + c.StartTime);
            ce.EndTime = DateTime.Parse(c.StartDate + " " + c.EndTime);
            ce.Busy = busy;
            if (c.StartDate == c.EndDate)
                ce.IsRecurring = false;
            else ce.IsRecurring = true;
            ce.RecurrenceConditions = "RRULE:FREQ=WEEKLY;UNTIL="+ DateTime.Parse(c.EndDate + " " + c.EndTime).ToString("s", System.Globalization.CultureInfo.InvariantCulture) + ";BYDAY=" + c.Weekday;
            if (reminder != null)
            {
                if (reminder.Split(' ')[1] == "0")
                    ce.Reminder = "BEGIN:VALARM\nACTION: DISPLAY\nDESCRIPTION:This is an event reminder\nTRIGGER:-P0DT0H" + reminder.Split(' ')[0] + "M0S\nEND:VALARM";
                else if (reminder.Split(' ')[1] == "1")
                    ce.Reminder = "BEGIN:VALARM\nACTION: DISPLAY\nDESCRIPTION:This is an event reminder\nTRIGGER:-P0DT" + reminder.Split(' ')[0] + "H0M0S\nEND:VALARM";
                else if (reminder.Split(' ')[1] == "2")
                    ce.Reminder = "BEGIN:VALARM\nACTION: DISPLAY\nDESCRIPTION:This is an event reminder\nTRIGGER:-P" + reminder.Split(' ')[0] + "DT0H0M0S\nEND:VALARM";

            }
            else ce.Reminder = "";
            return ce;
        }

        public static string replaceXMLTags(string s, Class c, string lec, string tut, string lab)
        {
            return s.Replace("<ClassType>", c.ClassType.Replace("Lecture", lec).Replace("Tutorial", tut).Replace("Laboratory", lab))
                .Replace("<CourseName>", c.CourseName)
                .Replace("<CourseCode>", c.CourseCode)
                .Replace("<CourseSection>", c.CourseSection)
                .Replace("<CRN>", c.CRN)
                .Replace("<Location>", c.Location)
                .Replace("<Instructor>", c.Instructor)
                .Replace("<StartTime>", c.StartTime)
                .Replace("<EndTime>", c.EndTime)
                .Replace("<WeekNumber>", c.WeekNumber);
        }

        public string GetVEventString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine("DTSTART;TZID=America/New_York:" + StartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendLine("DTEND;TZID=America/New_York:" + EndTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
            if(IsRecurring)
                sb.AppendLine(RecurrenceConditions);
            sb.AppendLine("DTSTAMP:" + DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "Z");
            sb.AppendLine("UID:" + DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "Z-" + (int)(new Random().NextDouble()*1000000) + "@example.com");
            sb.AppendLine("CREATED:19960329T133000Z");
            sb.AppendLine("DESCRIPTION:" + Description.Replace(Environment.NewLine, "\\n"));
            sb.AppendLine("SEQUENCE:0");
            sb.AppendLine("STATUS:CONFIRMED");
            sb.AppendLine("SUMMARY:" + Name);
            sb.AppendLine("TRANSP:" + (Busy ? "OPAQUE" : "TRANSPARENT"));
            if(Reminder != "")
                sb.AppendLine(Reminder);
            sb.AppendLine("END:VEVENT");
            return sb.ToString();
        }
    }
}
