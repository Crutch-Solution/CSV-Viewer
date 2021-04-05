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
        Thread _threadToKill;
        StatusStrip _statusStrip;
        public ProgressWindow(string header, string operation, Thread threadToKill, StatusStrip statusStrip)
        {
            InitializeComponent();
            Text = header;
            label1.Text = operation;
            _threadToKill = threadToKill;
            _statusStrip = statusStrip;
        }
        public void UpdateProgressBar(double percent)
        {
            if (percent == 100)
            {
                _statusStrip.Invoke((MethodInvoker)delegate () { _statusStrip.Items[0].Text = "Completed successfully"; });
                Close();
            }

            if (percent >= 0 && percent <= 100)
            {
                progressBar1.Value = (int)percent;
                label2.Text = $"{percent}%";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            _threadToKill.Abort();
            _statusStrip.Invoke((MethodInvoker)delegate () { _statusStrip.Items[0].Text = "Interrupted"; });
            Close();
        }

        private void ProgressWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            _threadToKill.Abort();
        }
    }
}
