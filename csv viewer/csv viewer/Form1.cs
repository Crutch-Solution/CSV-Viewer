using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace csv_viewer
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            graph2.BackColorLegend = Color.FromArgb(125, Color.Yellow);

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }
        
        public void callbackNames(List<string> names)
        {
            if (listBox1.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate () { callbackNames(names); });
            }
            else
            {
                lock (graph2)
                {
                    int selectedIndex = 0;
                    graph2.setChannelsNames(names);
                    Global.drawable.Clear();
                    foreach (var i in names)
                    {
                        listBox1.Items.Add(i);
                        if (selectedIndex < 5)
                            Global.drawable.Add(selectedIndex++);
                    }
                }
            }
        }
        public void callbackValues(List<List<PointF>> values)
        {
            if (listBox1.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate () { callbackValues(values); });
            }
            else
            {
                lock (graph2)
                {
                    graph2.setChannelsValues(values);
                }
            }
        }
        public void callbackCancel(string filename)
        {
            if (listBox1.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate () { callbackCancel(filename); });
            }
            else
            {
                listBox1.SelectedIndexChanged -= listBox1_SelectedIndexChanged;
                    statusStrip1.Items[0].Text = "Completed successfully";
                this.Text = $"CSV Viewer - {filename}";
                button2.Enabled = true;
                    foreach (var i in Global.drawable)
                        listBox1.SelectedIndices.Add(i);
                listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            statusStrip1.Items[0].Text = "Opening CSV file";
            OpenFileDialog file = new OpenFileDialog();
            if(file.ShowDialog() == DialogResult.OK)
            {
                statusStrip1.Items[0].Text = $"Opening CSV file: '{file.FileName}'";
                ProgressWindows PW = new ProgressWindows(file.FileName, this);
                
                new Thread(new ThreadStart((MethodInvoker)delegate() { Application.Run(PW); })).Start();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
                Global.drawable.Clear();
                foreach (var i in listBox1.SelectedIndices)
                {
                    Global.drawable.Add(Convert.ToInt32(i));
                }
                graph2.scale();
                graph2.draw();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            graph2.clear();
            Text = "CSV Viewer";
            statusStrip1.Items[0].Text = "Completed successfully";
            listBox1.Items.Clear();
            button2.Enabled = false;
        }
    }
}
