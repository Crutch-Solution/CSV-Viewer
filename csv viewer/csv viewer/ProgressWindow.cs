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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header">Window header</param>
        /// <param name="operation">Opeartion desription</param>
        /// <param name="threadToKill">Thread to be killed in case of interruption</param>
        /// <param name="statusStrip">Status field of a parent window (in case of interruption)</param>
        public ProgressWindow(string header, string operation, Thread threadToKill, StatusStrip statusStrip)
        {
            InitializeComponent();
            Text = header;
            label1.Text = operation;
            _threadToKill = threadToKill;
            _statusStrip = statusStrip;
        }
        /// <summary>
        /// Replaces current progress bar value and info label on 'percent', please round this input before calling)))
        /// </summary>
        /// <param name="percent"></param>
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
            _statusStrip.Invoke((MethodInvoker)delegate () 
            {
                _statusStrip.Items[0].Text = "Interrupted";
            });
            Close();
        }

        private void ProgressWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            _threadToKill.Abort();
        }
    }
}
