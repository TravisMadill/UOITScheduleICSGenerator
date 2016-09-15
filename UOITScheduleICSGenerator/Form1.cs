﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UOITScheduleICSGenerator
{
    public partial class Form1 : Form
    {

        Form_Format f;

        public static string loginURL = @"https://portal.mycampus.ca/cp/home/login";
        public static string scheduleURL = @"https://ssbp.mycampus.ca/prod_uoit/bwskfshd.P_CrseSchdDetl";
        WebClient client = new WebClient();

        public Form1()
        {
            InitializeComponent();

            
            //string reply = client.DownloadString(scheduleURL);
            //Console.WriteLine(reply);
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        private void button1_Click(object sender, EventArgs e)
        {
            var loginInfo = new System.Collections.Specialized.NameValueCollection();
            loginInfo.Add("userid", textBox1.Text);
            loginInfo.Add("pass", textBox2.Text);
            byte[] response = client.UploadValues(loginURL, "POST", loginInfo);
            string resp = Encoding.UTF8.GetString(response);
            Console.WriteLine(Encoding.UTF8.GetString(response));
            if (resp.Contains("<b>ERROR:</b>&nbsp;&nbsp;&nbsp;"))
            {
                MessageBox.Show(resp.Substring(resp.IndexOf("<b>ERROR:</b>&nbsp;&nbsp;&nbsp;")+3 + "<b>ERROR:</b>&nbsp;&nbsp;&nbsp;".Length, resp.IndexOf("</i><br>", resp.IndexOf("<b>ERROR:</b>&nbsp;&nbsp;&nbsp;")+3) - resp.IndexOf("<b>ERROR:</b>&nbsp;&nbsp;&nbsp;") - 11));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(f == null)
            f = new Form_Format();
            f.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string[] page = richTextBox1.Text.Split(new string[] { "<BR>" }, StringSplitOptions.None);

            for (int i = 0; i < page.Length - 1; i++)
                page[i] = Regex.Replace(page[i].Replace("\n", ""), "<[^>]+>", "ÿ").Replace(" (ÿPÿ)", "");

            List<Class> schedule = new List<Class>();

            for (int i = 2; i < page.Length - 1; i++)
            {
                string[] classInfo = page[i].Split('ÿ');
                Console.WriteLine();
                int tablePos = 0;
                bool hasPrev = true;
                string time = "";
                Class c = new Class();
                for (int j = 0; j < classInfo.Length; j++)
                {
                    if (classInfo[j].Equals(""))
                    {
                        hasPrev = false;
                        continue;
                    }
                    if (tablePos == 0)
                    {
                        string[] courseHeader = classInfo[j].Split(new string[] { " - " }, StringSplitOptions.None);
                        c.CourseName = courseHeader[0];
                        c.CourseCode = courseHeader[1];
                        if (courseHeader.Length > 2)
                            c.CourseSection = courseHeader[2];
                    }
                    else if (tablePos == 5)
                        c.CRN = classInfo[j];
                    else if(tablePos == 9)
                        c.Instructor = classInfo[j];
                    else if(tablePos > 20)
                    {
                        if ((tablePos - 21) % 8 == 0) // Week indicator
                        {
                            if (classInfo[j] == "W1" || classInfo[j] == "W2")
                                c.WeekNumber = classInfo[j];
                            else c.WeekNumber = "N/A";
                        }
                        else if ((tablePos - 21) % 8 == 2) // Class time
                            time = classInfo[j];
                        else if ((tablePos - 21) % 8 == 3) // Weekday
                            c.Weekday = classInfo[j];
                        else if ((tablePos - 21) % 8 == 4) // Location
                            c.Location = classInfo[j];
                        else if ((tablePos - 21) % 8 == 5) // Class dates
                            c.parseDateAndTime(classInfo[j], time);
                        else if ((tablePos - 21) % 8 == 6) // Class type
                            c.ClassType = classInfo[j];
                        else if ((tablePos - 21) % 8 == 7) // Technically instructor, but we got it earlier, so use this opporitunity to add other times for this class
                        {
                            schedule.Add(c.Clone());
                        }
                    }
                    tablePos++;
                    hasPrev = true;
                }
            }

            
        }
    }
}
