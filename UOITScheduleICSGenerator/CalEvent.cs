﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        public string UID { get; set; }

        public CalEvent() { }

        public static CalEvent classAsCalEvent(Class c)
        {
            CalEvent ce = new CalEvent();
            string title, desc, loc, lec, tut, lab, reminder = null;
            bool busy, showRNumOnly;
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
                    showRNumOnly = br.ReadBoolean();
                }
            }
            catch (IOException)
            {
                title = "<ClassType>: <CourseName> (<CRN>)";
                desc = @"Course code: <CourseCode>\nCRN: <CRN>";
                loc = "<Location>";
                lec = "Lec.";
                tut = "Tut.";
                lab = "Lab";
                reminder = "20 0";
                busy = true;
                showRNumOnly = false;
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

            ce.Name = replaceXMLTags(title, c, lec, tut, lab, showRNumOnly);
            ce.Description = replaceXMLTags(desc, c, lec, tut, lab, showRNumOnly);
            ce.Location = replaceXMLTags(loc, c, lec, tut, lab, showRNumOnly);
            c.StartDate = getProperDate(c.StartDate, c.Weekday); //Start event on proper day, rather than everything starting on the first day
            ce.StartTime = DateTime.Parse(c.StartDate + " " + c.StartTime);
            ce.EndTime = DateTime.Parse(c.StartDate + " " + c.EndTime);
            ce.UID = c.CRN;
            ce.Busy = busy;
            if (c.StartDate == c.EndDate)
                ce.IsRecurring = false;
            else ce.IsRecurring = true;
            ce.RecurrenceConditions = "RRULE:FREQ=WEEKLY;UNTIL="+ DateTime.Parse(c.EndDate + " " + c.EndTime).AddHours(5).ToString("yyyyMMddTHHmmssZ") + ";BYDAY=" + c.Weekday;
            if (reminder != null)
            {
                if (reminder.Split(' ')[1] == "0")
                    ce.Reminder = "BEGIN:VALARM\r\nACTION:DISPLAY\r\nDESCRIPTION:This is an event reminder\r\nTRIGGER:-P0DT0H" + reminder.Split(' ')[0] + "M0S\r\nEND:VALARM";
                else if (reminder.Split(' ')[1] == "1")
                    ce.Reminder = "BEGIN:VALARM\r\nACTION:DISPLAY\r\nDESCRIPTION:This is an event reminder\r\nTRIGGER:-P0DT" + reminder.Split(' ')[0] + "H0M0S\r\nEND:VALARM";
                else if (reminder.Split(' ')[1] == "2")
                    ce.Reminder = "BEGIN:VALARM\r\nACTION:DISPLAY\r\nDESCRIPTION:This is an event reminder\r\nTRIGGER:-P" + reminder.Split(' ')[0] + "DT0H0M0S\r\nEND:VALARM";

            }
            else ce.Reminder = "";
            return ce;
        }

        private static string getProperDate(string startDate, string weekday)
        {
            DateTime dt = DateTime.Parse(startDate);
            DayOfWeek d = DayOfWeek.Monday;
            switch (weekday)
            {
                case "MO": d = DayOfWeek.Monday; break;
                case "TU": d = DayOfWeek.Tuesday; break;
                case "WE": d = DayOfWeek.Wednesday; break;
                case "TH": d = DayOfWeek.Thursday; break;
                case "FR": d = DayOfWeek.Friday; break;
                case "SA": d = DayOfWeek.Saturday; break;
                case "SU": d = DayOfWeek.Sunday; break;
            }
            int daysToAdd = (d - dt.DayOfWeek + 7) % 7;
            return dt.AddDays(daysToAdd).ToShortDateString();
        }

        public static string replaceXMLTags(string s, Class c, string lec, string tut, string lab, bool showRNumOnly)
        {
            return s.Replace("<ClassType>", c.ClassType.Replace("Lecture", lec).Replace("Tutorial", tut).Replace("Laboratory", lab))
                .Replace("<CourseName>", c.CourseName)
                .Replace("<CourseCode>", c.CourseCode)
                .Replace("<CourseSection>", c.CourseSection)
                .Replace("<CRN>", c.CRN)
                .Replace("<Location>", showRNumOnly ? getLocationAcronym(c) : getLocationFullName(c))
                .Replace("<Instructor>", c.Instructor)
                .Replace("<StartTime>", c.StartTime)
                .Replace("<EndTime>", c.EndTime)
                .Replace("<WeekNumber>", c.WeekNumber);
        }

        public static string getLocationAcronym(Class c)
        {
            string s = c.Location.TrimEnd(' ');
            string rm = c.Location.Split(' ')[c.Location.Split(' ').Length - 1];
            try { s = s.Remove(s.LastIndexOf(' ')); } catch (Exception) { }
            while (!Char.IsNumber(rm[0]) && rm.Length > 1)
                rm = rm.Substring(1);

            switch (s)
            {
                case "Science Building (UA)":
                    return "UA " + rm;
                case "Business and IT Building (UB)":
                    return "UB " + rm;
                case "Energy Research Centre (ERC)":
                    return "ERC " + rm;
                case "Software and Informatics Resea":
                    return "SIRC " + rm;
                case "UL Building":
                    return "UL " + rm;
                case "Simcoe Building/J-Wing":
                    return "Simcoe J " + rm;
                case "OPG Engineering Building":
                    return "ENG " + rm;
                case "University Pavilion":
                    return "UP" + rm;
                case "61 Charles Street Building":
                    return "DTA " + rm;
                case "Bordessa Hall":
                    return "DTB " + rm;
                case "Regent Theatre":
                    return "DTR " + rm;
                case "Education Building":
                    return "EDU " + rm;
                case "Georgian College":
                    return "Georgian College (" + rm + ")";
                case "Off Site Location":
                    return "Offsite location";
                case "Virtual Adobe Connect":
                    return "Online (" + rm + ")";
                case "N/A":
                    return "N/A";
                case "TBA":
                    return "TBA";
                default:
                    System.Diagnostics.Debug.WriteLine("Unknown acronym for " + s);
                    break;
            }
            return c.Location;
        }

        public static string getLocationFullName(Class c)
        {
            string s = c.Location.TrimEnd(' ');
            string rm = c.Location.Split(' ')[c.Location.Split(' ').Length - 1];
            try { s = s.Remove(s.LastIndexOf(' ')); } catch (Exception) { }
            while (!Char.IsNumber(rm[0]) && rm.Length > 1)
                rm = rm.Substring(1);

            switch (s)
            {
                case "Science Building (UA)":
                    return "Science Bldg., room " + rm;
                case "Business and IT Building (UB)":
                    return "Business & IT Bldg., room " + rm;
                case "Energy Research Centre (ERC)":
                    return "Energy Research Ctr., room " + rm;
                case "Software and Informatics Resea":
                    return "Software & Informatics Research Bldg., room " + rm;
                case "UL Building":
                    return "Library Portables, room " + rm;
                case "Simcoe Building/J-Wing":
                    return "Simcoe Bldg., J Wing, room " + rm;
                case "OPG Engineering Building":
                    return "Engineering Bldg., room " + rm;
                case "University Pavilion":
                    return "University Pavilion " + rm;
                case "61 Charles Street Building":
                    return "61 Charles St. (Dtwn. Bldg. A), room " + rm;
                case "Bordessa Hall":
                    return "Bordessa Hall (Dtwn. Bldg. B), room " + rm;
                case "Regent Theatre":
                    return "Regent Theatre, room " + rm;
                case "Education Building":
                    return "Education Bldg., room " + rm;
                case "Georgian College":
                    return "Georgian College (" + rm + ")";
                case "Off Site Location":
                    return "Offsite location";
                case "Virtual Adobe Connect":
                    return "Online Adobe Connect class #" + rm;
                case "N/A":
                    return "Not available";
                case "TBA":
                    return "Not yet determined";
                default:
                    System.Diagnostics.Debug.WriteLine("Unknown location: " + s);
                    break;
            }
            return c.Location;
        }

        public string GetVEventString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine("DTSTART;TZID=America/New_York:" + StartTime.ToString("yyyyMMddTHHmmss"));
            sb.AppendLine("DTEND;TZID=America/New_York:" + EndTime.ToString("yyyyMMddTHHmmss"));
            if(IsRecurring)
                sb.AppendLine(RecurrenceConditions);
            sb.AppendLine("DTSTAMP:" + DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"));
            using (MD5 hash = MD5.Create())
            {
                sb.AppendLine("UID:" + string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(StartTime.ToString("yyyyMMddTHHmmssZ") + "@" + UID)).Select(x => x.ToString("X2"))));
            }
            sb.AppendLine("CREATED:19960329T133000Z");
            sb.AppendLine("DESCRIPTION:" + Description.Replace(Environment.NewLine, "\\n"));
            sb.AppendLine("LOCATION:" + Location);
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
