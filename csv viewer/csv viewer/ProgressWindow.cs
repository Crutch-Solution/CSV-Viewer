using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
namespace csv_viewer
{
    public partial class ProgressWindow : Form
    {
        public Thread ThreadToKill;
        public ProgressWindow(string header, string operation, Thread threadToKill)
        {
            InitializeComponent();
            Text = header;
            label1.Text = operation;
            ThreadToKill = threadToKill;
        }
        public void UpdateProgressBar(double percent)
        {
            if (percent == 100)
                Close();
            if (percent >= 0 && percent <= 100)
            {
                progressBar1.Value = (int)percent;
                label2.Text = $"{percent}%";
            }
            Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ThreadToKill.Abort();
            Close();
        }

        private void ProgressWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            ThreadToKill.Abort();
        }
    }
}
