using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOITScheduleICSGenerator
{
    class Class
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Weekday { get; set; }
        public string RecurrenceRules { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string CourseSection { get; set; }
        public string CRN { get; set; }
        public string WeekNumber { get; set; }
        public string Location { get; set; }
        public string ClassType { get; set; }
        public string Instructor { get; set; }

        public Class() { }
        
        public bool parseDateAndTime(string date, string time)
        {
            StartDate = date.Split(new string[] { " - " }, StringSplitOptions.None)[0];
            EndDate = date.Split(new string[] { " - " }, StringSplitOptions.None)[1];
            StartTime = time.Split(new string[] { " - " }, StringSplitOptions.None)[0];
            EndTime = time.Split(new string[] { " - " }, StringSplitOptions.None)[1];
            return true;
        }
        public Class Clone()
        {
            Class b = new Class();
            b.StartDate = StartDate;
            b.EndTime = EndDate;
            b.StartTime = StartTime;
            b.EndTime = EndTime;
            b.Weekday = Weekday;
            b.RecurrenceRules = RecurrenceRules;
            b.CourseName = CourseName;
            b.CourseCode = CourseCode;
            b.CourseSection = CourseSection;
            b.CRN = CRN;
            b.WeekNumber = WeekNumber;
            b.Location = Location;
            b.ClassType = ClassType;
            b.Instructor = Instructor;
            return b;
        }
    }
}
