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
        public bool head;

        public Form1()
        {
            InitializeComponent();

            head = true;
            
            //string reply = client.DownloadString(scheduleURL);
            //Console.WriteLine(reply);
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        private void button1_Click(object sender, EventArgs e)
        {
            //Console.WriteLine(webBrowser1.DocumentText);
            var loginInfo = new System.Collections.Specialized.NameValueCollection();
            loginInfo.Add("cplogin.userid", textBox1.Text);
            loginInfo.Add("cplogin.pass", textBox2.Text);
            loginInfo.Add("cplogin.uuid", "0xACA021");
            client.Headers.Add(HttpRequestHeader.Cookie, "");
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
            if (f == null)
                f = new Form_Format();
            else if (f.IsDisposed)
                f = new Form_Format();
            f.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!webBrowser1.DocumentText.Contains("<title>Student Detail Schedule</title>"))
            {
                MessageBox.Show("This is not the correct webpage." +
                    (webBrowser1.DocumentText.Contains("<frameset") ? "\n\nIf you ARE on the schedule page, press the \"Go to schedule page\" button so that the data can be read properly.\n\n...Yeah, it's a dumb thing to do, but the fact that this app needs to exist in order for this to work properly suggests otherwise."
                    : "\n\nMake sure you're on the \"Detailed Schedule\" page,\nand NOT the \"Schedule by Date & Time\" page."));
                Console.WriteLine(webBrowser1.DocumentText);
            }
            else
            {
                //Split every section by a line break command.
                string[] page = webBrowser1.DocumentText.Split(new string[] { "<BR>" }, StringSplitOptions.None);

                // Strip all HTML tags from each split.
                for (int i = 0; i < page.Length - 1; i++)
                    page[i] = Regex.Replace(page[i].Replace("\n", ""), "<[^>]+>", "ÿ").Replace(" (ÿPÿ)", "").Replace("ÿTBAÿ", "TBA");

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
                        //else System.Diagnostics.Debug.WriteLine(tablePos + " " + classInfo[j]);

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
                        else if (tablePos > 19)
                        {
                            if ((tablePos - 20) % 7 == 0) // Week indicator
                            {
                                if (classInfo[j].Contains("W1") || classInfo[j].Contains("W2"))
                                    c.WeekNumber = classInfo[j];
                                else c.WeekNumber = "N/A";
                            }
                            if ((tablePos - 20) % 7 == 1) // Class time
                            {
                                time = classInfo[j];
                                if (time == "TBA")
                                    notTBA = false;
                            }
                            else if ((tablePos - 20) % 7 == 2 && notTBA) // Weekday
                            {
                                c.Weekday = classInfo[j];
                                if (time == "TBA")
                                    notTBA = false;
                            }
                            else if ((tablePos - 20) % 7 == 3 && notTBA) // Location
                                c.Location = classInfo[j];
                            else if ((tablePos - 20) % 7 == 4 && notTBA) // Class dates
                                c.parseDateAndTime(classInfo[j], time);
                            else if ((tablePos - 20) % 7 == 5 && notTBA) // Class type
                                c.ClassType = classInfo[j];
                            else if ((tablePos - 20) % 7 == 6) // Technically instructor, but we got it earlier, so use this opporitunity to add other times for this class
                            {
                                if (notTBA) //If this course's times are TBA (usually for online courses), then don't add them to the schedule.
                                    schedule.Add(c.Clone());
                            }
                        }
                        tablePos++;
                    }
                }
                List<CalEvent> events = new List<CalEvent>();
                foreach (Class c in schedule)
                    events.Add(CalEvent.classAsCalEvent(c));
                content = CalFile.CreateICSFileContents(events);

                saveFileDialog1.ShowDialog();
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
            HtmlElementCollection hec = webBrowser1.Document.All;
            Console.WriteLine(webBrowser1.DocumentText.Substring(0, 100000));
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (fh == null)
                fh = new Form_HowTo();
            else if (fh.IsDisposed)
                fh = new Form_HowTo();
            fh.Show();
        }

        private void resizeWindow(object sender, EventArgs e)
        {
            Form currentForm = sender as Form;
            webBrowser1.Size = new Size(currentForm.Size.Width - 40, currentForm.Size.Height - 120);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (head && webBrowser1.Url.ToString().Equals(@"https://uoit.ca/mycampus/"))
            {
                //Strip down all the stuff that's not the login form.
                string text = webBrowser1.DocumentText;
                int start = 0;
                int start_end = text.IndexOf("<!-- Off-canvas wrapper -->");
                int form = text.IndexOf("<!--START LOGIN AREA-->", start_end);
                int form_end = text.IndexOf("<br/>", form);
                int end = text.IndexOf("<script src=\"https://shared.uoit.ca/global-2.1/dist/js/global.js\" type=\"text/javascript\"></script>", form_end);
                int end_end = text.Length;

                webBrowser1.DocumentText = text.Substring(start, start_end) + text.Substring(form, form_end - form) + text.Substring(end, end_end - end);
                head = false;
            }
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (!webBrowser1.Url.ToString().Equals(@"https://uoit.ca/mycampus/"))
                head = true;
        }
    }
}
