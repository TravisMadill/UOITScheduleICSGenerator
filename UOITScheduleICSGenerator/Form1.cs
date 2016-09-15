using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UOITScheduleICSGenerator
{
    public partial class Form1 : Form
    {
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
    }
}
