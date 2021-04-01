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
        public void openfile()
        {
            List<List<PointF>> values = new List<List<PointF>>();
            using (StreamReader reader = new StreamReader(Path))
            {
                int prev = 0;
                string line = reader.ReadLine();
                current = line.Length;
                List<string> names = new List<string>();
                for (int i = 0; i < line.Split('\t').Length - 1; i++)
                {
                    names.Add(line.Split('\t')[i + 1]);
                    values.Add(new List<PointF>());
                }

                callBackNames(names);

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
                            values[i].Add(new PointF(Convert.ToSingle(line.Split('\t')[0]), Convert.ToSingle(line.Split('\t')[i + 1])));
                            // label1.Text = i.ToString();
                        }
                    }catch(Exception ex)
                    {

                    }
                    current += line.Length;

                    if ((current - prev) / (maximum*1.0f) * 100 > 5)
                    {
                        prev = current;
                        callBackValues(values);
                        foreach (var i in values)
                            i.Clear();
                    }



                }
                   
            }
        }
        delegate void delNames(List<string> names);
        delegate void delValues(List<List<PointF>> values);
        public void callBackNames(List<string> names)
        {
            if (progressBar1.InvokeRequired)
            {
                Invoke(new delNames(callBackNames), names);
            }
            else
            {
                progressBar1.Value = current;
                progressBar1.Refresh();
                father.callbackNames(names);
            }
        }
        public void callBackValues(List<List<PointF>> values)
        {
            if (progressBar1.InvokeRequired)
            {
                Invoke(new delValues(callBackValues), values);
            }
            else
            {
                progressBar1.Value = current;
                progressBar1.Refresh();
                father.callbackValues(values);
            }
        }
        int temp = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            main.Abort();
        }
    }
}
