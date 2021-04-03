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
        public ProgressWindow(string header, string operation)
        {
            InitializeComponent();
            Text = header;
            label1.Text = operation;
        }
        public void UpdateProgressBar(int percent)
        {
            if (percent == 100)
                Close();
            if (percent >= 0 && percent <= 100)
                progressBar1.Value = percent;
            Refresh();
        }
    }
}
