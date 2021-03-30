using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
        public void callback(List<Channel> list)
        {
            graph2.setChannesl(list);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            if(file.ShowDialog() == DialogResult.OK)
            {
                long size = new FileInfo(file.FileName).Length;
                ProgressWindows PW = new ProgressWindows(file.FileName, this);
                if(PW.ShowDialog() == DialogResult.OK)
                {

                }
            }
        }
    }
}
