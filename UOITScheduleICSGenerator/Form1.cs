using System;
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
using System.IO;

namespace UOITScheduleICSGenerator
{
    public partial class Form1 : Form
    {

        Form_Format f;
        Form_HowTo fh;

        public static string termSelectURL = @"https://ssbp.mycampus.ca/prod_uoit/bwskflib.P_SelDefTerm2";
        public static string loginURL = @"https://portal.mycampus.ca/cp/home/login";
        public static string scheduleURL = @"https://ssbp.mycampus.ca/prod_uoit/bwskfshd.P_CrseSchdDetl";
        public static string content;
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
            Console.WriteLine(webBrowser1.DocumentText);
            /*var loginInfo = new System.Collections.Specialized.NameValueCollection();
            loginInfo.Add("userid", textBox1.Text);
            loginInfo.Add("pass", textBox2.Text);
            byte[] response = client.UploadValues(loginURL, "POST", loginInfo);
            string resp = Encoding.UTF8.GetString(response);
            Console.WriteLine(Encoding.UTF8.GetString(response));
            if (resp.Contains("<b>ERROR:</b>&nbsp;&nbsp;&nbsp;"))
            {
                MessageBox.Show(resp.Substring(resp.IndexOf("<b>ERROR:</b>&nbsp;&nbsp;&nbsp;")+3 + "<b>ERROR:</b>&nbsp;&nbsp;&nbsp;".Length, resp.IndexOf("</i><br>", resp.IndexOf("<b>ERROR:</b>&nbsp;&nbsp;&nbsp;")+3) - resp.IndexOf("<b>ERROR:</b>&nbsp;&nbsp;&nbsp;") - 11));
            }*/
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (f == null)
                f = new Form_Format();
            else if (f.IsDisposed)
                f = new Form_Format();
            f.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!webBrowser1.DocumentText.Contains("<TITLE>Student Detail Schedule</TITLE>"))
                MessageBox.Show("This is not the correct webpage.");
            else
            {
                //Split every section by a line break command.
                string[] page = webBrowser1.DocumentText.Split(new string[] { "<BR>" }, StringSplitOptions.None);

                // Strip all HTML tags from each split.
                for (int i = 0; i < page.Length - 1; i++)
                    page[i] = Regex.Replace(page[i].Replace("\n", ""), "<[^>]+>", "ÿ").Replace(" (ÿPÿ)", "");

                //Start going through each course table and adding them to the master schedule.
                List<Class> schedule = new List<Class>();
                for (int i = 2; i < page.Length - 1; i++)
                {
                    string[] classInfo = page[i].Split('ÿ');
                    //Console.WriteLine(); //(For breakpoint)
                    int tablePos = 0;
                    string time = "";
                    bool notTBA = true;
                    Class c = new Class();
                    for (int j = 0; j < classInfo.Length; j++)
                    {
                        if (classInfo[j].Equals(""))
                            continue;

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
                        else if (tablePos == 9)
                            c.Instructor = classInfo[j];
                        else if (tablePos > 20)
                        {
                            if ((tablePos - 21) % 8 == 0) // Week indicator
                            {
                                if (classInfo[j] == "W1" || classInfo[j] == "W2")
                                    c.WeekNumber = classInfo[j];
                                else c.WeekNumber = "N/A";
                            }
                            else if ((tablePos - 21) % 8 == 2) // Class time
                            {
                                time = classInfo[j];
                                if (time == "TBA")
                                    notTBA = false;
                            }
                            else if ((tablePos - 21) % 8 == 3 && notTBA) // Weekday
                            {
                                c.Weekday = classInfo[j];
                                if (time == "TBA")
                                    notTBA = false;
                            }
                            else if ((tablePos - 21) % 8 == 4 && notTBA) // Location
                                c.Location = classInfo[j];
                            else if ((tablePos - 21) % 8 == 5 && notTBA) // Class dates
                                c.parseDateAndTime(classInfo[j], time);
                            else if ((tablePos - 21) % 8 == 6 && notTBA) // Class type
                                c.ClassType = classInfo[j];
                            else if ((tablePos - 21) % 8 == 7) // Technically instructor, but we got it earlier, so use this opporitunity to add other times for this class
                            {
                                if (notTBA) //If this course's times are TBA (usually for online courses), then don't add them to the schedule.
                                    schedule.Add(c.Clone());
                            }
                        }
                        tablePos++;
                    }
                }
                List<CalEvent> events = new List<CalEvent>();
                foreach(Class c in schedule)
                    events.Add(CalEvent.classAsCalEvent(c));
                content = CalFile.CreateICSFileContents(events);

                saveFileDialog1.ShowDialog();
                
                MessageBox.Show(":)");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            webBrowser1.Url = new Uri(scheduleURL);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            webBrowser1.Url = new Uri(termSelectURL);
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (!e.Cancel && content != null)
            {
                using (StreamWriter w = new StreamWriter(File.Open(saveFileDialog1.FileName, FileMode.Create)))
                {
                    w.Write(content);
                    w.Close();
                }
                MessageBox.Show("Saved.");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoBack)
                webBrowser1.GoBack();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            webBrowser1.Url = new Uri(@"C:\Users\100547276\Downloads\new 1.html");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (fh == null)
                fh = new Form_HowTo();
            else if (fh.IsDisposed)
                fh = new Form_HowTo();
            fh.Show();
        }
    }
}
