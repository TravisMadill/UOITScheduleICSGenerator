using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace UOITScheduleICSGenerator
{
    public partial class Form_Format : Form
    {
        public static string settingFilePath = Application.StartupPath + @"\settings.txt";

        public Form_Format()
        {
            InitializeComponent();
            if(File.Exists(settingFilePath))
            {
                try
                {
                    using (BinaryReader br = new BinaryReader(File.Open(settingFilePath, FileMode.Open)))
                    {
                        textBox1.Text = br.ReadString(); //Title
                        textBox2.Text = br.ReadString(); //Description
                        textBox3.Text = br.ReadString(); //Location
                        textBox4.Text = br.ReadString(); //Lecture
                        textBox5.Text = br.ReadString(); //Tutorial
                        textBox6.Text = br.ReadString(); //Laboratory
                        checkBox2.Checked = br.ReadBoolean(); //Busy
                        if (br.ReadBoolean()) //Reminders
                        {
                            checkBox1.Checked = true;
                            numericUpDown1.Value = br.ReadDecimal();
                            comboBox1.SelectedIndex = br.ReadInt32();
                        }
                        else checkBox1.Checked = false;

                        br.Close();
                    }
                }
                catch (IOException)
                {
                    textBox1.Text = "<ClassType>: <CourseName> (<CRN>)"; //Title
                    textBox2.Text = "Course code: <CourseCode>-<CourseSection>" + Environment.NewLine + "CRN: <CRN>"; //Description
                    textBox3.Text = "<Location>"; //Location
                    textBox4.Text = "Lec."; //Lecture
                    textBox5.Text = "Tut."; //Tutorial
                    textBox6.Text = "Lab"; //Laboratory
                    checkBox2.Checked = true; //Busy
                    checkBox1.Checked = true;
                    numericUpDown1.Value = 20;
                    comboBox1.SelectedIndex = 0;
                }
            }
            else
                File.Create(settingFilePath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!File.Exists(settingFilePath))
                File.Create(settingFilePath);
            using(BinaryWriter bw = new BinaryWriter(File.Open(settingFilePath, FileMode.Open)))
            {
                bw.Write(textBox1.Text);
                bw.Write(textBox2.Text);
                bw.Write(textBox3.Text);
                bw.Write(textBox4.Text);
                bw.Write(textBox5.Text);
                bw.Write(textBox6.Text);
                bw.Write(checkBox2.Checked);
                bw.Write(checkBox1.Checked);
                if (checkBox1.Checked)
                {
                    bw.Write(numericUpDown1.Value);
                    bw.Write(comboBox1.SelectedIndex);
                }
                bw.Close();
            }
            MessageBox.Show(":)");
            Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                numericUpDown1.Enabled = true;
                comboBox1.Enabled = true;
            }
            else
            {
                numericUpDown1.Enabled = false;
                comboBox1.Enabled = false;
            }
        }
    }
}
