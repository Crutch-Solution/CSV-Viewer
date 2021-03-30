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
    public partial class ProgressWindows : Form
    {
        delegate void del();
        string Path;
        Form1 father;
        int maximum;
        int current;
        Thread main;
        public ProgressWindows(string path, Form1 father)
        {
            InitializeComponent();
            Path = path;
            this.father = father;
            progressBar1.Maximum = (int)new FileInfo(Path).Length;
            maximum = progressBar1.Maximum;
            progressBar1.Value = 0;
            List<string> lines = new List<string>();
            main = new Thread(new ThreadStart(openfile));
            main.Start();
        }
        public List<Channel> list;
        public void openfile()
        {
            list = new List<Channel>();
            using (StreamReader reader = new StreamReader(Path))
            {
                string line = reader.ReadLine();
                for (int i= 0; i< line.Split('\t').Length-1; i++)
                    list.Add(new Channel(line.Split('\t')[i+1], Global.LegendPens[i % Global.Colors]));

                int prev = 0;
                current = line.Length;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    try
                    {
                        for (int i = 0; i < line.Split('\t').Length - 1; i++)
                        {
                            try
                            {
                                Convert.ToSingle(line.Split('\t')[i + 1]);
                            }
                            catch
                                (Exception ex)
                            {
                                continue;
                            }
                            list[i].add(new PointF(Convert.ToSingle(line.Split('\t')[0]), Convert.ToSingle(line.Split('\t')[i + 1])));
                            // label1.Text = i.ToString();
                        }
                    }catch(Exception ex)
                    {

                    }
                    current += line.Length;

                    if ((current - prev) / (maximum*1.0f) * 100 > 0)
                    {
                        prev = current;
                        callBack();
                    }



                }
                   
            }
        }
        public void callBack()
        {
            if (progressBar1.InvokeRequired)
            {
                Invoke(new del(callBack));
            }
            else
            {
                progressBar1.Value = current;
                progressBar1.Refresh();
                father.callback(list);
                label1.Text = (temp++).ToString();
            }
        }
        int temp = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            main.Abort();
        }
    }
}
